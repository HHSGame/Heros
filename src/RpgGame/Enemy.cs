using System;
using System.Collections.Generic;
using Terminal.Gui;

namespace RpgGame
{
    public enum EnemyType
    {
        Goblin,
        Orc,
        Troll
    }

    public enum EnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Fleeing
    }

    public class Enemy
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public int Strength { get; private set; }
        public int Defense { get; private set; }
        public int ExperienceValue { get; private set; }
        public string? Name { get; private set; }
        public EnemyType Type { get; private set; }
        public EnemyState State { get; private set; }
        
        private int _attackCooldown;
        private int _specialAbilityCooldown;
        private Random _random;

        private static readonly Dictionary<EnemyType, (string Name, int MaxHealth, int Strength, int Defense, int ExperienceValue)> _enemyStats = new()
        {
            { 
                EnemyType.Goblin, 
                ("Goblin", 30, 5, 2, 50) 
            },
            { 
                EnemyType.Orc, 
                ("Orc", 60, 8, 5, 100) 
            },
            { 
                EnemyType.Troll, 
                ("Troll", 100, 12, 8, 200) 
            }
        };

        public Enemy(EnemyType type, int x, int y)
        {
            _random = new Random();
            Type = type;
            X = x;
            Y = y;
            State = EnemyState.Chasing;

            if (_enemyStats.TryGetValue(type, out var stats))
            {
                Name = stats.Name;
                MaxHealth = stats.MaxHealth;
                Strength = stats.Strength;
                Defense = stats.Defense;
                ExperienceValue = stats.ExperienceValue;
            }
            
            Health = MaxHealth;
            _attackCooldown = 0;
            _specialAbilityCooldown = 0;
        }

        public void Update(Player player)
        {
            if (_attackCooldown > 0) _attackCooldown--;
            if (_specialAbilityCooldown > 0) _specialAbilityCooldown--;
            
            // Calculate squared distance to player (avoid expensive sqrt)
            int dx = player.X - X;
            int dy = player.Y - Y;
            int sqDistance = dx * dx + dy * dy;

            // Update state based on squared distance
            if (sqDistance <= 1) // 1^2 = 1
            {
                State = EnemyState.Attacking;
            }
            else if (sqDistance <= 64) // 8^2 = 64
            {
                State = EnemyState.Chasing;
            }
            else
            {
                State = EnemyState.Idle;
            }

            // Handle state-specific behavior
            switch (State)
            {
                case EnemyState.Attacking:
                    if (_attackCooldown <= 0)
                    {
                        Attack(player);
                        _attackCooldown = 2; // 2 turn cooldown
                    }
                    break;
                    
                case EnemyState.Chasing:
                    // Move towards player
                    int moveX = Math.Sign(dx);
                    int moveY = Math.Sign(dy);
                    Move(moveX, moveY);
                    break;
            }
        }

        private static readonly Dictionary<EnemyType, (int Chance, Action<Player, Enemy> Effect)> _attackEffects = new()
        {
            { 
                EnemyType.Goblin, 
                (20, (player, self) => player.ApplyEffect(new PoisonEffect(3))) 
            },
            { 
                EnemyType.Orc, 
                (10, (player, self) => player.ApplyEffect(new StunEffect(1))) 
            },
            { 
                EnemyType.Troll, 
                (30, (player, self) => self.Heal(10)) 
            }
        };

        public void Attack(Player player)
        {
            int damage = Strength;
            
            // Apply type-specific effects
            if (_attackEffects.TryGetValue(Type, out var effectData) && 
                _random.Next(100) < effectData.Chance)
            {
                effectData.Effect(player, this);
            }
            
            player.TakeDamage(damage);
        }

        public void TakeDamage(int damage)
        {
            int actualDamage = damage - Defense;
            if (actualDamage < 0) actualDamage = 0;
            Health -= actualDamage;
            
            if (Health <= 0)
            {
                Health = 0;
                Die();
            }
        }

        public void Heal(int amount)
        {
            Health += amount;
            if (Health > MaxHealth) Health = MaxHealth;
        }

        public void Move(int dx, int dy)
        {
            X += dx;
            Y += dy;
        }

        private void Die()
        {
            // Drop loot
            var loot = GenerateLoot();
            // TODO: Add loot to game world
        }

        private static readonly Dictionary<EnemyType, (int Chance, int Amount)> _lootTable = new()
        {
            { EnemyType.Goblin, (30, 10) },
            { EnemyType.Orc, (50, 20) },
            { EnemyType.Troll, (70, 30) }
        };

        public List<Item> GenerateLoot()
        {
            var loot = new List<Item>();
            var roll = _random.Next(100);
            
            if (_lootTable.TryGetValue(Type, out var lootData) && roll < lootData.Chance)
            {
                loot.Add(new HealthPotion(lootData.Amount));
            }
            
            return loot;
        }
        
        public void Draw(char[,] buffer)
        {
            char symbol = Type switch
            {
                EnemyType.Goblin => 'g',
                EnemyType.Orc => 'o',
                EnemyType.Troll => 'T',
                _ => 'E'
            };
                
            buffer[Y, X] = symbol;
        }

        public void Clear(char[,] buffer)
        {
            buffer[Y, X] = ' ';
        }
    }
}
