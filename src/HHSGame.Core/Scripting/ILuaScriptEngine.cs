namespace HHSGame.Core.Scripting
{
    /// <summary>
    /// Lua 脚本引擎接口
    /// </summary>
    public interface ILuaScriptEngine : IDisposable
    {
        /// <summary>
        /// 加载脚本
        /// </summary>
        void LoadScript(string name, string code);

        /// <summary>
        /// 执行脚本
        /// </summary>
        void Execute(string script);

        /// <summary>
        /// 执行脚本并返回结果
        /// </summary>
        T? Execute<T>(string script);

        /// <summary>
        /// 调用全局函数
        /// </summary>
        T? CallFunction<T>(string functionName, params object[] args);

        /// <summary>
        /// 设置全局变量
        /// </summary>
        void SetGlobal(string name, object value);

        /// <summary>
        /// 获取全局变量
        /// </summary>
        T? GetGlobal<T>(string name);

        /// <summary>
        /// 检查脚本是否有语法错误
        /// </summary>
        bool ValidateScript(string script, out string? error);

        /// <summary>
        /// 清除所有加载的脚本
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// 暴露给 Lua 的游戏 API
    /// </summary>
    public interface IGameLuaAPI
    {
        // 玩家
        int GetPlayerX();
        int GetPlayerY();
        int GetPlayerHP();
        int GetPlayerMaxHP();
        int GetPlayerAP();
        void HealPlayer(int amount);
        void DamagePlayer(int amount);
        void GiveItem(string itemId, int quantity);
        void RemoveItem(string itemId, int quantity);
        bool HasItem(string itemId);

        // 地图
        void SetTile(int x, int y, char tile);
        char GetTile(int x, int y);
        void RevealArea(int x, int y, int radius);
        bool IsWalkable(int x, int y);
        int GetMapWidth();
        int GetMapHeight();

        // 敌人
        void SpawnEnemy(string enemyId, int x, int y);
        void RemoveEnemy(int x, int y);
        int GetEnemyCount();
        bool IsEnemyAt(int x, int y);

        // NPC
        void SpawnNpc(string npcId, int x, int y);
        void StartDialogue(string dialogueId);
        bool IsNpcAt(int x, int y);

        // 任务
        void StartQuest(string questId);
        void CompleteQuest(string questId);
        bool IsQuestActive(string questId);
        bool IsQuestComplete(string questId);
        void SetQuestObjectiveProgress(string questId, int objectiveIndex, int value);

        // 声望
        int GetReputation(string faction);
        void ModifyReputation(string faction, int amount);

        // 消息
        void ShowMessage(string message);
        void ShowNotification(string message);

        // 地图切换
        void ChangeMap(string mapId, int x, int y);

        // 触发器
        void EnableTrigger(string triggerId);
        void DisableTrigger(string triggerId);
        void RegisterTrigger(string triggerId, string triggerType, string script);

        // 工具
        int Random(int min, int max);
        bool RollCheck(int difficulty);
        int GetTurnCount();
        string GetLocalizedString(string key);
    }
}
