/*
 * Copyright © 2012 Nataly Akentyeva. All rights reserved.

 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see http://www.gnu.org/licenses/
 * */
using System;
using System.ComponentModel;

namespace pMonstersBase
{
    class Hero : INotifyPropertyChanged
    {
        String name;
        String kindOf;
        UInt32 level;
        UInt32 balls;
        Decimal money;
        UInt32 health;
        Double exp;
        Double chance;
        Boolean dead;

        String gameLog;
        //разделитель полей при сериализации
        static char delim = ';';

        /// <summary>
        /// Конструктор пустого героя.
        /// </summary>
        public Hero() { }

        /// <summary>
        /// Конструктор героя.
        /// </summary>
        public Hero(String name, String cl, UInt32 lvl, UInt32 balls, Decimal money, UInt32 health, Double exp, Double chance)
        {
            this.name = name;
            this.kindOf = cl;
            this.level = lvl;
            this.balls = balls;
            this.money = money;
            this.exp = exp;
            this.chance = chance;
            this.health = health;
            this.dead = false;
            this.gameLog = "";
        }
        //------------------------------------------------------------------
        //  Сеттеры и геттеры
        //------------------------------------------------------------------
        public UInt32 hDelim
        {
            get { return delim; }
        }

        public String vName
        {
            get { return name; }
            set { name = value; }
        }

        public String vClass
        {
            get { return kindOf.ToString(); }
            set { kindOf = value; }
        }

        public String vLevel
        {
            get { return level.ToString(); }
            set { level = Convert.ToUInt32(value); OnPropertyChanged("vLevel"); }
        }

        public String vHealth
        {
            get { return health.ToString(); }
            set { health = Convert.ToUInt32(value); OnPropertyChanged("vHealth"); }
        }

        public String vBalls
        {
            get { return balls.ToString(); }
            set { balls = Convert.ToUInt32(value); }
        }

        public String vMoney
        {
            get { return money.ToString("0.0"); }
            set { money = Convert.ToDecimal(value); OnPropertyChanged("vMoney"); }
        }
        public String vExp
        {
            get { return exp.ToString("0.00"); }
            set { exp = Convert.ToDouble(value); }
        }
        public String vChance
        {
            get { return chance.ToString("0%"); }
            set { chance = Convert.ToDouble(value.Replace("%", "")) / 100; }
        }

        public Boolean isDead
        {
            get { return dead; }
            set { dead = Convert.ToBoolean(value); }
        }

        public String hGameLog
        {
            get { return gameLog; }
            set { gameLog += value + Environment.NewLine + Environment.NewLine; OnPropertyChanged("hGameLog"); }
        }

        //для быстрого доступа к числовым значениям
        public int dLevel
        {
            get { return (int)level; }
            set { level = Convert.ToUInt32(value); }
        }

        public UInt32 dHealth
        {
            get { return health; }
            set { health = Convert.ToUInt32(value); }
        }

        public UInt32 dBalls
        {
            get { return balls; }
            set { balls = Convert.ToUInt32(value); }
        }

        public Decimal dMoney
        {
            get { return money; }
            set { money = Convert.ToDecimal(value); }
        }
        public Double dExp
        {
            get { return exp; }
            set { exp = Convert.ToDouble(value); }
        }
        public Double dChance
        {
            get { return chance; }
        }
        
        //очистка лога игры
        public void clearGameLog()
        {
            gameLog = "";
        }
        //------------------------------------------------------------------
        //  Механизм оповещения UI Bindings об изменениях свойств
        //------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        //------------------------------------------------------------------
        //  Сериализация данных
        //------------------------------------------------------------------
        public string ToString(char del)
        {
            return Serialize(del);

        }
        public override string ToString()
        {
            return Serialize(delim);

        }
        private string Serialize(char del)
        {
            return this.vName + del + this.vClass + del + this.vLevel + del + this.vBalls + del
                + this.vMoney + del + this.vHealth + del + this.vExp + del + this.vChance + del
                + this.isDead.ToString();
        }

        /// <summary>
        /// Генерация героя заданного класса.
        /// </summary>
        public static Hero randomByClass(String name, String cl)
        {
            //Запускаем рандомайзер...
            Random rand = new Random(DateTime.Now.Millisecond);

            //Генерим общие свойства нового героя             
            Decimal nMoney = 0;
            Double nExp = 0;
            UInt32 nLevel = 0;
            UInt32 nBalls = 0;
            UInt32 nHealth = 0;
            Double nChance = 0;

            //Генерация прочих свойств в зависимости от класса
            switch (cl)
            {
                case "rincewind":
                    nLevel = 1;
                    nBalls = 0;
                    nHealth = 100;
                    nChance = 0.95;
                    break;
                case "kohen":
                    nLevel = 10;
                    nBalls = 4;
                    nHealth = 50;
                    nChance = 0.85;
                    break;
                case "lemming":
                    nLevel = 1;
                    nBalls = 5;
                    nHealth = 50;
                    nChance = 0.45;
                    break;
                case "ninja":
                    nLevel = 1;
                    nBalls = 3;
                    nHealth = 100;
                    nChance = 0.75;
                    break;
                case "usual":
                default:
                    nLevel = 0;
                    nBalls = (UInt32)rand.Next(0, 6); ;
                    nHealth = 100;
                    nChance = rand.NextDouble();
                    break;
            }
            
            return new Hero(name, cl, nLevel, nBalls, nMoney, nHealth, nExp, nChance);
        }
        /// <summary>
        /// Встреча героя с монстром. На основании параметров того и другого решается, быть ли бою
        /// </summary>
        public void monsterMeet( Monster m)
        {
            if ((m.dLevel - this.dLevel) > 4)
            {
                this.hGameLog = m.vName
                + " оказался слишком занят и не заметил мельтешащего под ногами " + this.vName;

                if (this.dBalls > (m.dLevel - this.dLevel) * this.dChance)
                {
                    this.hGameLog = "Обиженный этим обстоятельством герой наступил монстру на хвост "
                        + "и приготовился дорого продать свою жизнь.";
                    this.monsterFight(m);
                }
                return;
            }
            else
            {
                String baldness = this.dBalls >= 4 ? " злорадно ухмыльнулся" : " на всякий случай побледнел";
                this.hGameLog = m.vName
                + ", заметив героя, плотоядно заурчал и облизнулся. "
                + this.vName + baldness + " и покрепче схватился за меч.";

                this.monsterFight(m);
            }

        }
        /// <summary>
        /// Бой героя с монстром. На основании параметров того и другого решается, быть ли бою
        /// </summary>
        private void monsterFight(Monster m)
        {
            if ((this.dLevel * this.dChance - (m.dLevel * m.dChance + m.age / 3)) > 0)
            {
                //герой победил
                this.vMoney = (this.dMoney + m.dPrice).ToString();
                this.vExp = (this.dExp + m.dExp).ToString();
                int newHealth = (int)(this.dHealth - (m.dLevel + m.age));
                this.vHealth = newHealth < 0 ? "0" : newHealth.ToString();

                this.hGameLog = this.vName + " героически убил " + m.vName + ", пополнив свое богатство на " + m.vPrice
                    + ", опыт - на " + m.vExp + " пунктов. Здоровье уменьшилось до " + this.vHealth + ".";

                if (this.dHealth <= 0)
                {
                    this.isDead = true;
                    return;
                }
                if (this.dExp > 20)
                {
                    //прибавляем герою уровень, урезаем опыт
                    this.vLevel = (this.dLevel + 1).ToString();
                    this.dExp -= 17;                    
                    this.vHealth = 
                        (this.dHealth + 2 * this.dLevel) > 100 
                        ? "100"
                        : (this.dHealth + 2 * this.dLevel).ToString();
                    this.hGameLog = "Ура! Герой получил новый уровень - " + this.vLevel 
                        + ", заодно и подлечил здоровье до " + this.vHealth + "%";
                }
            }
            else
            {
                this.vHealth = (this.dHealth - 2 * m.dLevel) < 0
                        ? "0"
                        : Math.Floor(this.dHealth - m.dExp * m.dLevel).ToString();
                String entry = "Чувствуя, что проигрывает, " + this.vName + " бросился наутек, теряя тапки и здоровье (" 
                    + this.vHealth + "%).";
                if (this.dHealth == 0)
                {
                    this.isDead = true;
                    entry += " От чего и умер.";
                    this.hGameLog = entry;
                    return;
                }
                if (m.vPredator == "1" && ((this.dLevel * this.dChance + this.dHealth / 20) < m.dLevel * m.dChance))
                {
                    //не повезло убежать
                    entry += " Но " + m.vName + ", к несчастью, оказался хищником, и к тому же очень шустрым."
                        + " И явно голодным. ";
                    entry += m.vName + " загрыз " + this.vName;
                    this.isDead = true;
                }
                else
                {
                    entry += ", монстр пожал плечами и продолжил щипать траву.";
                }
                this.hGameLog = entry;
            }
        }
    }
}
