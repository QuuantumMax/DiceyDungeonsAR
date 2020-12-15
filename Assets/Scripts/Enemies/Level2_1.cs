﻿using DiceyDungeonsAR.Battle;

namespace DiceyDungeonsAR.Enemies
{
    public class Level2_1 : Enemy
    {
        public override int Level { get; } = 2;
        public override int MaxHealth { get; } = 26;

        public override void FillInventory()
        {
            Cards[0, 0] = new CardDescription()
            {
                action = CardAction.Damage,
                bonus = new Bonus() { type = BonusType.Shock },
                condition = new Condition() { type = ConditionType.Even },
            };
        }
    }
}
