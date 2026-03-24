using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Threading;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Runs;
using MQTTnet;

namespace MqttTheSpire;

[ModInitializer(nameof(Entrypoint))]
public static class MqttTheSpire
{
    private static RunManager _runManager;
    private static IMqttClient _mqttClient;
    private static string _modDirectory;
    private static RunState _runState;
    private static MqttTheSpireConfig _config;
    private static Player _player;

    /// <summary>
    /// Loads additional assemblies before initializing the mod.
    /// </summary>
    public static void Entrypoint()
    {
        _modDirectory = GetModDirectory();
        _config = LoadConfig();
        LoadAdditionalAssemblies();
        Initialize();
    }

    private static void Initialize()
    {
        try
        {
            _runManager = RunManager.Instance;
            _mqttClient = CreateMqttClient();
            BindRunManagerEvents();
        }
        catch (Exception e)
        {
            LogError(e.ToString());
            throw;
        }
    }

    #region Events

    private static void BindRunManagerEvents()
    {
        _runManager.RunStarted += OnRunStarted;
        // _runManager.ActEntered += OnActEntered;
        _runManager.RoomEntered += OnRoomEntered;
        // _runManager.RoomExited += OnRoomExited;
    }

    private static void BindRunStateEvents()
    {
        _player.GoldChanged += OnPlayerGoldChanged;
        _player.Creature.MaxHpChanged += OnPlayerMaxHpChanged;
        _player.Creature.CurrentHpChanged += OnPlayerHpChanged;
        _player.Creature.BlockChanged += OnPlayerBlockChanged;
    }

    private static void OnRunStarted(RunState runState)
    {
        _runState = runState;
        _player = LocalContext.GetMe(_runState);
        PublishRunStart();
        PublishAscensionLevel();
        PublishCharacter();
        PublishGameMode();
        BindRunStateEvents();
    }

    private static void OnRoomEntered()
    {
        PublishTotalFloor();
        PublishRoomType();
    }

    private static void OnRoomExited()
    {
        PublishTotalFloor();
    }

    private static void OnActEntered()
    {
        PublishAct();
    }

    private static void OnPlayerGoldChanged()
    {
        PublishPlayerGold();
    }

    private static void OnPlayerMaxHpChanged(int target, int current)
    {
        PublishPlayerMaxHp();
    }

    private static void OnPlayerHpChanged(int target, int current)
    {
        PublishPlayerHp();
    }

    private static void OnPlayerBlockChanged(int target, int current)
    {
        PublishPlayerBlock();
    }

    #endregion

    #region MqttTopics

    private static void PublishTotalFloor() => PublishMqttTopic("run/total_floor", _runState?.TotalFloor);
    private static void PublishAct() => PublishMqttTopic("run/current_act", _runState?.CurrentActIndex);
    private static void PublishRunStart() => PublishMqttTopic("run/start_time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    private static void PublishAscensionLevel() => PublishMqttTopic("run/ascension_level", _runState?.AscensionLevel);
    private static void PublishCharacter() => PublishMqttTopic("run/player/character", _player.Character.Id.Entry);
    private static void PublishRoomType() => PublishMqttTopic("run/room_type", _runState?.CurrentRoom?.RoomType);
    private static void PublishGameMode() => PublishMqttTopic("run/game_mode", _runState?.GameMode);
    private static void PublishPlayerGold() => PublishMqttTopic("run/player/gold", _player.Gold);
    private static void PublishPlayerMaxHp() => PublishMqttTopic("run/player/max_hp", _player.Creature.MaxHp);
    private static void PublishPlayerHp() => PublishMqttTopic("run/player/current_hp", _player.Creature.CurrentHp);
    private static void PublishPlayerBlock() => PublishMqttTopic("run/player/current_block", _player.Creature.Block);

    #endregion

    #region Helpers

    private static string GetModDirectory()
    {
        var modDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        LogInfo($"Mod directory: {modDirectory}");
        return modDirectory;
    }

    private static MqttTheSpireConfig LoadConfig()
    {
        var config =
            JsonSerializer.Deserialize<MqttTheSpireConfig>(File.Open(Path.Combine(_modDirectory, "config"),
                FileMode.Open));
        LogInfo($"MQTT CONFIG: {config}");
        return config;
    }

    private static void LoadAdditionalAssemblies()
    {
        var assemblyLoadContext = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
        assemblyLoadContext!.LoadFromAssemblyPath(Path.Combine(_modDirectory!, "MQTTnet.dll"));
    }

    private static void PublishMqttTopic(string subtopic, object status)
    {
        var topic = $"{_config.Topic}/{subtopic}";
        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(status?.ToString())
            .Build();
        _mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
        LogInfo($"MQTT TOPIC PUBLISHED: {topic} - {status}");
    }

    private static IMqttClient CreateMqttClient()
    {
        var mqttFactory = new MqttClientFactory();
        var mqttClient = mqttFactory.CreateMqttClient();

        var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(_config.Host, _config.Port)
            .WithCredentials(_config.User, _config.Password)
            .Build();

        var connectionResultTask = mqttClient.ConnectAsync(mqttClientOptions);
        connectionResultTask.Wait();

        if (connectionResultTask.Result.ResultCode != MqttClientConnectResultCode.Success)
            throw new Exception($"MQTT CONNECTION FAILED: {connectionResultTask.Result.ReasonString}");

        LogInfo("MQTT CLIENT CONNECTED");

        return mqttClient;
    }

    private static void LogInfo(string message) => Log.Info($"MqttTheSpire: {message}");
    private static void LogWarn(string message) => Log.Warn($"MqttTheSpire: {message}");
    private static void LogError(string message) => Log.Error($"MqttTheSpire: {message}");

    #endregion
}