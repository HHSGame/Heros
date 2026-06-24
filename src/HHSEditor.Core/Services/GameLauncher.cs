using System.Diagnostics;

namespace HHSEditor.Core.Services;

/// <summary>
/// 游戏启动服务
/// </summary>
public sealed class GameLauncher
{
    private Process? _gameProcess;
    private readonly IProjectService _projectService;

    public event EventHandler<string>? OutputReceived;
    public event EventHandler<string>? ErrorReceived;
    public event EventHandler<int>? GameExited;

    public bool IsRunning => _gameProcess != null && !_gameProcess.HasExited;

    public GameLauncher(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// 启动游戏进行测试
    /// </summary>
    public async Task<bool> LaunchGameAsync(CancellationToken ct = default)
    {
        if (IsRunning)
        {
            OutputReceived?.Invoke(this, "Game is already running.");
            return false;
        }

        string? projectPath = _projectService.CurrentProjectPath;
        if (projectPath == null)
        {
            ErrorReceived?.Invoke(this, "No project loaded. Please open a project first.");
            return false;
        }

        // 查找 game.json
        string gameJsonPath = FindGameJsonPath(projectPath);
        if (gameJsonPath == null)
        {
            ErrorReceived?.Invoke(this, "Cannot find game.json in project.");
            return false;
        }

        // 查找 HHSGame 项目
        string gameProjectPath = FindGameProjectPath();
        if (gameProjectPath == null)
        {
            ErrorReceived?.Invoke(this, "Cannot find HHSGame project.");
            return false;
        }

        try
        {
            OutputReceived?.Invoke(this, $"Starting game with config: {gameJsonPath}");
            OutputReceived?.Invoke(this, $"Game project: {gameProjectPath}");

            // 使用 shell 执行方式启动，这样可以显示终端窗口
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{gameProjectPath}\" -- --config \"{gameJsonPath}\"",
                UseShellExecute = true,  // 改为 true 以显示窗口
                CreateNoWindow = false,
                WorkingDirectory = Path.GetDirectoryName(gameProjectPath)
            };

            _gameProcess = new Process { StartInfo = startInfo };

            _gameProcess.Exited += (sender, args) =>
            {
                int exitCode = _gameProcess?.ExitCode ?? -1;
                GameExited?.Invoke(this, exitCode);
                _gameProcess?.Dispose();
                _gameProcess = null;
            };

            _gameProcess.EnableRaisingEvents = true;
            _gameProcess.Start();

            OutputReceived?.Invoke(this, "Game process started. Check for the game window.");
            OutputReceived?.Invoke(this, "If the window doesn't appear, try running manually:");
            OutputReceived?.Invoke(this, $"  dotnet run --project \"{gameProjectPath}\" -- --config \"{gameJsonPath}\"");

            return true;
        }
        catch (Exception ex)
        {
            ErrorReceived?.Invoke(this, $"Failed to start game: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 启动游戏（终端模式）
    /// </summary>
    public async Task<bool> LaunchGameInTerminalAsync(CancellationToken ct = default)
    {
        if (IsRunning)
        {
            OutputReceived?.Invoke(this, "Game is already running.");
            return false;
        }

        string? projectPath = _projectService.CurrentProjectPath;
        if (projectPath == null)
        {
            ErrorReceived?.Invoke(this, "No project loaded.");
            return false;
        }

        string gameJsonPath = FindGameJsonPath(projectPath);
        if (gameJsonPath == null)
        {
            ErrorReceived?.Invoke(this, "Cannot find game.json.");
            return false;
        }

        string gameProjectPath = FindGameProjectPath();
        if (gameProjectPath == null)
        {
            ErrorReceived?.Invoke(this, "Cannot find HHSGame project.");
            return false;
        }

        try
        {
            // 先构建项目
            OutputReceived?.Invoke(this, "Building game...");
            var buildResult = await RunCommandAsync("dotnet", $"build \"{gameProjectPath}\" --verbosity quiet", Path.GetDirectoryName(gameProjectPath));

            if (buildResult.ExitCode != 0)
            {
                ErrorReceived?.Invoke(this, "Build failed:");
                ErrorReceived?.Invoke(this, buildResult.Output);
                return false;
            }

            OutputReceived?.Invoke(this, "Build successful. Starting game...");

            // 启动游戏（不重定向输出，让 Terminal.Gui 正常工作）
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{gameProjectPath}\" --no-build -- --config \"{gameJsonPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = false,
                WorkingDirectory = Path.GetDirectoryName(gameProjectPath)
            };

            _gameProcess = new Process { StartInfo = startInfo };

            _gameProcess.Exited += (sender, args) =>
            {
                int exitCode = _gameProcess?.ExitCode ?? -1;
                GameExited?.Invoke(this, exitCode);
                _gameProcess?.Dispose();
                _gameProcess = null;
            };

            _gameProcess.EnableRaisingEvents = true;
            _gameProcess.Start();

            OutputReceived?.Invoke(this, "Game started successfully!");
            OutputReceived?.Invoke(this, "The game window should appear now.");

            return true;
        }
        catch (Exception ex)
        {
            ErrorReceived?.Invoke(this, $"Failed to start game: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 停止游戏
    /// </summary>
    public void StopGame()
    {
        if (_gameProcess != null && !_gameProcess.HasExited)
        {
            try
            {
                _gameProcess.Kill(true);
                OutputReceived?.Invoke(this, "Game stopped.");
            }
            catch (Exception ex)
            {
                ErrorReceived?.Invoke(this, $"Failed to stop game: {ex.Message}");
            }
        }
    }

    private async Task<(int ExitCode, string Output)> RunCommandAsync(string fileName, string arguments, string? workingDirectory = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return (process.ExitCode, string.IsNullOrEmpty(output) ? error : output);
    }

    private string? FindGameJsonPath(string projectPath)
    {
        string[] possiblePaths =
        [
            Path.Combine(projectPath, "game.json"),
            Path.Combine(projectPath, "data", "game.json")
        ];

        foreach (string path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    private string? FindGameProjectPath()
    {
        // 从当前目录向上查找 HHSGame.csproj
        string currentDir = Directory.GetCurrentDirectory();
        string[] searchPaths =
        [
            Path.Combine(currentDir, "src", "HHSGame", "HHSGame.csproj"),
            Path.Combine(currentDir, "..", "src", "HHSGame", "HHSGame.csproj"),
            Path.Combine(currentDir, "..", "..", "src", "HHSGame", "HHSGame.csproj"),
            "/Users/qliu23/workspace/fe/Slime/src/HHSGame/HHSGame.csproj"
        ];

        foreach (string path in searchPaths)
        {
            string fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }

    public void Dispose()
    {
        StopGame();
        _gameProcess?.Dispose();
    }
}
