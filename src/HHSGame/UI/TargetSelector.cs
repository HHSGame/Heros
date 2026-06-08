using HHSGame.Core;
using HHSGame.Core.Combat;
using HHSGame.Core.Enemies;
using HHSGame.Core.Npcs;
using HHSGame.Core.Rendering;
using Terminal.Gui.Input;

namespace HHSGame.UI
{
    /// <summary>
    /// 处理目标选择状态（移动/攻击/对话）。
    /// 从 GameUI 中提取，减少 GameUI 的职责。
    /// </summary>
    public sealed class TargetSelector(Game game)
    {
        private readonly Game game = game;
        // 移动选择
        private bool isMoveSelection;
        private Coordinate moveTarget = new(0, 0);

        // 攻击选择
        private bool isAttackSelection;
        private readonly List<Enemy> attackTargets = [];
        private int attackTargetIndex;

        // 对话选择
        private bool isTalkSelection;
        private readonly List<Npc> talkTargets = [];
        private Npc? selectedTalkTarget;

        // 闪烁状态
        private bool selectionBlinkOn;

        public bool IsMoveSelection => isMoveSelection;
        public bool IsAttackSelection => isAttackSelection;
        public bool IsTalkSelection => isTalkSelection;
        public bool IsAnySelection => isMoveSelection || isAttackSelection || isTalkSelection;

        public Coordinate MoveTarget => moveTarget;
        public Enemy? CurrentAttackTarget => isAttackSelection && attackTargets.Count > 0 ? attackTargets[attackTargetIndex] : null;
        public Npc? SelectedTalkTarget => selectedTalkTarget;
        public IReadOnlyList<Enemy> AttackTargets => attackTargets;
        public IReadOnlyList<Npc> TalkTargets => talkTargets;

        // ── 移动选择 ──────────────────────────────────────────────────

        public void BeginMoveSelection(Coordinate playerPosition)
        {
            isMoveSelection = true;
            moveTarget = playerPosition;
        }

        public void CancelMoveSelection()
        {
            isMoveSelection = false;
        }

        public void UpdateMoveTarget(Coordinate delta, bool isInBounds)
        {
            Coordinate newTarget = new(moveTarget.X + delta.X, moveTarget.Y + delta.Y);
            if (isInBounds)
            {
                moveTarget = newTarget;
            }
        }

        // ── 攻击选择 ──────────────────────────────────────────────────

        public void BeginAttackSelection(List<Enemy> targets)
        {
            attackTargets.Clear();
            attackTargets.AddRange(targets);
            isAttackSelection = true;
            attackTargetIndex = 0;
        }

        public void CancelAttackSelection()
        {
            isAttackSelection = false;
            attackTargets.Clear();
            attackTargetIndex = 0;
        }

        public void NextAttackTarget()
        {
            if (attackTargets.Count > 1)
            {
                attackTargetIndex = (attackTargetIndex + 1) % attackTargets.Count;
            }
        }

        // ── 对话选择 ──────────────────────────────────────────────────

        public void BeginTalkSelection(List<Npc> targets)
        {
            talkTargets.Clear();
            talkTargets.AddRange(targets);
            isTalkSelection = true;
            selectedTalkTarget = talkTargets.Count > 0 ? talkTargets[0] : null;
        }

        public void CancelTalkSelection()
        {
            isTalkSelection = false;
            talkTargets.Clear();
            selectedTalkTarget = null;
        }

        public void NextTalkTarget()
        {
            if (talkTargets.Count > 1 && selectedTalkTarget != null)
            {
                int currentIndex = talkTargets.IndexOf(selectedTalkTarget);
                int nextIndex = (currentIndex + 1) % talkTargets.Count;
                selectedTalkTarget = talkTargets[nextIndex];
            }
        }

        // ── 闪烁控制 ──────────────────────────────────────────────────

        public bool ToggleSelectionBlink()
        {
            selectionBlinkOn = !selectionBlinkOn;
            return selectionBlinkOn;
        }

        // ── 获取目标预览 ──────────────────────────────────────────────

        public List<(Coordinate Position, Cell Cell)> GetTargetPreviewCells()
        {
            List<(Coordinate Position, Cell Cell)> cells = [];

            if (isMoveSelection)
            {
                cells.Add((moveTarget, new Cell
                {
                    Character = GUISettings.PlannedDestinationGlyph,
                    Attribute = ColorPresets.PathPreview
                }));
            }

            if (isAttackSelection && CurrentAttackTarget != null)
            {
                cells.Add((CurrentAttackTarget.Position, new Cell
                {
                    Character = GUISettings.TargetPreviewGlyph,
                    Attribute = selectionBlinkOn ? ColorPresets.TargetPreviewHighlight : ColorPresets.TargetPreview
                }));
            }

            return cells;
        }
    }
}
