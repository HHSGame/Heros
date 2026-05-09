using HHSGame.Core.Tutorial;

namespace HHSGame.Core.Tutorial
{
    /// <summary>
    /// Provides default tutorial scenarios for new players.
    /// </summary>
    public static class TutorialScenarios
    {
        public static TutorialScenario CreateDefaultScenario()
        {
            return new TutorialScenario
            {
                Id = "default-tutorial",
                Name = "入门引导",
                Description = "学习游戏基础操作",
                Steps =
                [
                    new TutorialStep
                    {
                        Id = "welcome",
                        Title = "欢迎来到沦陷区",
                        Message = "欢迎！使用方向键在地图上移动。试着向任意方向移动。",
                        Trigger = TutorialTrigger.OnMove,
                        Order = 0
                    },
                    new TutorialStep
                    {
                        Id = "explore",
                        Title = "探索",
                        Message = "你正在探索沦陷区。四处走动发现地图上的事物。物品以字母显示在地图上。",
                        Trigger = TutorialTrigger.OnTurnEnd,
                        Order = 1
                    },
                    new TutorialStep
                    {
                        Id = "pickup",
                        Title = "拾取物资",
                        Message = "按 G 键拾取地面上的物资。物资将存入你的背包。",
                        Trigger = TutorialTrigger.OnItemPickup,
                        Order = 2
                    },
                    new TutorialStep
                    {
                        Id = "inventory",
                        Title = "背包",
                        Message = "按 I 键打开背包。按回车键使用物品。再按 I 键关闭。",
                        Trigger = TutorialTrigger.OnInventoryOpen,
                        Order = 3
                    },
                    new TutorialStep
                    {
                        Id = "combat",
                        Title = "遭遇战",
                        Message = "发现敌人！在战斗中，按 M 规划移动，A 规划攻击，然后回车确认。或按 C 切换战斗模式。",
                        Trigger = TutorialTrigger.OnCombatStart,
                        Order = 4
                    },
                    new TutorialStep
                    {
                        Id = "combat_toggle",
                        Title = "战斗模式",
                        Message = "按 C 在战斗和探索模式之间切换。探索模式下移动即时生效，战斗模式下需规划后确认。",
                        Trigger = TutorialTrigger.OnCombatToggle,
                        Order = 5
                    },
                    new TutorialStep
                    {
                        Id = "skills",
                        Title = "技能",
                        Message = "按 S 打开技能菜单。技能包括检查、搜索、潜行等。战斗中每项技能消耗行动点。",
                        Trigger = TutorialTrigger.OnSkillUse,
                        Order = 6
                    },
                    new TutorialStep
                    {
                        Id = "npc",
                        Title = "NPC",
                        Message = "按 T 与附近的 NPC 交谈。他们可以提供任务和交易物资。",
                        Trigger = TutorialTrigger.OnNpcTalk,
                        Order = 7
                    },
                    new TutorialStep
                    {
                        Id = "interact",
                        Title = "互动对象",
                        Message = "使用检查和使用对象技能与箱子、书籍、拉杆等互动。",
                        Trigger = TutorialTrigger.OnObjectInteract,
                        Order = 8
                    },
                    new TutorialStep
                    {
                        Id = "save",
                        Title = "存档",
                        Message = "按 Ctrl+S 保存游戏，Ctrl+L 读取存档。教程完成！祝你好运。",
                        Trigger = TutorialTrigger.OnSaveOrLoad,
                        Order = 9
                    }
                ]
            };
        }
    }
}