using System.Diagnostics;
using System.IO;

namespace AutoChess.Services;

public sealed class StockfishAdapter : IDisposable
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    private Process? _process;
    private TaskCompletionSource<bool>? _uciOkTcs;
    private TaskCompletionSource<bool>? _readyOkTcs;
    private TaskCompletionSource<string>? _bestMoveTcs;
    private bool _disposed;

    public record UciCommand(string Value)
    {
        public static readonly UciCommand Uci = new("uci");
        public static readonly UciCommand IsReady = new("isready");
        public static readonly UciCommand NewGame = new("ucinewgame");
        public static readonly UciCommand Quit = new("quit");

        public static UciCommand PositionFen(string fen) => new($"position fen {fen}");
        public static UciCommand GoMoveTime(int ms) => new($"go movetime {ms}");

        public override string ToString() => Value;
        public static implicit operator string(UciCommand cmd) => cmd.Value;
    }

    private static ReadOnlySpan<char> UciOk => "uciok";
    private static ReadOnlySpan<char> ReadyOk => "readyok";
    private static ReadOnlySpan<char> BestMovePrefix => "bestmove";

    public bool IsRunning => _process is { HasExited: false };

    public async Task StartAsync(string executablePath, CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (IsRunning)
        {
            return;
        }

        CreateProcess(executablePath);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(DefaultTimeout);

        _uciOkTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await SendAndAwaitResponse(UciCommand.Uci, _uciOkTcs, cts.Token);

        _readyOkTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await SendAndAwaitResponse(UciCommand.IsReady, _readyOkTcs, cts.Token);
    }

    public async Task NewGameAsync(CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(DefaultTimeout);

        SendLine(UciCommand.NewGame);
        _readyOkTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await SendAndAwaitResponse(UciCommand.IsReady, _readyOkTcs, cts.Token);
    }

    public async Task<string> GetBestMoveAsync(string fen, int moveTimeMs = 1000, CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!IsRunning)
        {
            throw new InvalidOperationException(GameStatusManager.Exceptions.EngineNotRunning);
        }

        if (_bestMoveTcs is { Task.IsCompleted: false })
        {
            throw new InvalidOperationException(GameStatusManager.Exceptions.QueryStillPending);
        }

        if (moveTimeMs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(moveTimeMs), "Move time must be positive.");
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromMilliseconds(moveTimeMs + 5000));

        _bestMoveTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        SendLine(UciCommand.PositionFen(fen));
        return await SendAndAwaitResponse(UciCommand.GoMoveTime(moveTimeMs), _bestMoveTcs, cts.Token);
    }

    private void OnOutputLine(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Data))
        {
            return;
        }

        ReadOnlySpan<char> span = e.Data.AsSpan().Trim();

        if (span.SequenceEqual(UciOk))
        {
            _uciOkTcs?.TrySetResult(true);
        }
        else if (span.SequenceEqual(ReadyOk))
        {
            _readyOkTcs?.TrySetResult(true);
        }
        else if (span.StartsWith(BestMovePrefix))
        {
            var movePart = span.Slice(BestMovePrefix.Length).TrimStart();
            int spaceIndex = movePart.IndexOf(' ');
            var move = spaceIndex == -1 ? movePart : movePart.Slice(0, spaceIndex);
            _bestMoveTcs?.TrySetResult(move.ToString());
        }
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        CancelAllPending();
    }

    private void CancelAllPending()
    {
        _uciOkTcs?.TrySetCanceled();
        _readyOkTcs?.TrySetCanceled();
        _bestMoveTcs?.TrySetCanceled();
    }

    private void SendLine(UciCommand command)
    {
        if (_process?.StandardInput is { } input)
        {
            input.WriteLine(command.ToString());
        }
    }

    private async Task<T> SendAndAwaitResponse<T>(UciCommand command, TaskCompletionSource<T> tcs, CancellationToken ct)
    {
        using var registration = ct.Register(() => tcs.TrySetCanceled(ct));
        SendLine(command);
        return await tcs.Task;
    }

    private void CreateProcess(string executablePath)
    {
        if (!File.Exists(executablePath))
        {
            throw new FileNotFoundException(GameStatusManager.Exceptions.EngineNotFound(executablePath));
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            },
            EnableRaisingEvents = true
        };

        process.OutputDataReceived += OnOutputLine;
        process.ErrorDataReceived += (_, _) => { };
        process.Exited += OnProcessExited;

        try
        {
            process.Start();
        }
        catch
        {
            process.Dispose();
            throw;
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        _process = process;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        CancelAllPending();

        if (_process == null)
        {
            return;
        }

        try
        {
            if (!_process.HasExited)
            {
                SendLine(UciCommand.Quit);
                if (!_process.WaitForExit(2000))
                {
                    _process.Kill();
                }
            }
        }
        catch (InvalidOperationException)
        {
            // noop
        }
        finally
        {
            _process.Dispose();
            _process = null;
        }
    }
}
