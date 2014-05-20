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

namespace pMonstersBase
{
    class Monster
    {
        UInt32 id;
        String name;
        UInt32 level;
        Decimal price;
        Double exp;
        Double chance;
        Boolean predator;
        DateTime date;
        
        // переменная класса - хранит id крайнего монстра (kinda serial)
        static UInt32 lastId;
        //разделтель полей при сериализации
        static char delim = ';';

        /// <summary>
        /// Конструктор пустого монстра.
        /// </summary>
        public Monster() { }

        /// <summary>
        /// Конструктор непустого монстра.
        /// </summary>
        public Monster(String name, UInt32 lvl, Decimal price, Double expa, Double chance, Boolean predator, DateTime date)
        {
            this.id = lastId + 1;
            this.name = name;
            this.level = lvl;
            this.price = price;
            this.exp = expa;
            this.chance = chance;
            this.predator = predator;
            this.date = date;

            lastId = this.id;
        }
        //------------------------------------------------------------------
        //  Сеттеры и геттеры
        //------------------------------------------------------------------
        public UInt32 mLastId
        {
            get { return lastId; }
            set { lastId = Convert.ToUInt32(value); }
        }

        public UInt32 mDelim
        {
            get { return delim; }
        }

        public String vId
        {
            get { return id.ToString(); }
            set { id = Convert.ToUInt32(value); }
        }
        public String vName
        {
            get { return name; }
            set { name = value; }
        }
        public String vLevel
        {
            get { return level.ToString(); }
            set { level = Convert.ToUInt32(value); }
        }
        public String vPrice
        {
            get { return price.ToString("0.0"); }
            set { price = Convert.ToDecimal(value); }
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

        public String vPredator
        {
            get { return predator ? "1" : "0"; }
            set { predator = Convert.ToBoolean(Convert.ToUInt32(value)); }
        }
        public String vDate
        {
            get { return date.ToString(); }
            set { date = Convert.ToDateTime(value); }
        }
        public String vDay
        {
            get { return date.Date.ToString("dd"); }
        }
        public String vMonth
        {
            get { return date.Date.ToString("MM"); }
        }
        public String vYear
        {
            get { return date.Date.ToString("yy"); }
        }
        //для быстрого доступа к числовым значениям
        public int dLevel
        {
            get { return (int)level; }
            set { level = Convert.ToUInt32(value); }
        }
        public Decimal dPrice
        {
            get { return price; }
            set { price = Convert.ToDecimal(value); }
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
        //вычисление возраста монстра
        public UInt32 age
        {
            get
            {
                DateTime now = DateTime.Today;
                int age = now.Year - date.Year;
                if (date > now.AddYears(-age)) age--;
                return Convert.ToUInt32(age);
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
            return this.vId + del + this.vName + del + this.vLevel + del + this.vPrice
                + del + this.vExp + del + this.vChance + del + this.vPredator + del + this.vDate;
        }
        /// <summary>
        /// Генерация монстра с произвольными свойствами.
        /// </summary>
        public static Monster random()
        {
            //Запускаем рандомайзер...
            Random rand = new Random(DateTime.Now.Millisecond);
            String name = RandomName(rand.Next(3, 15));

            //Генерим свойства нового монстра 
            UInt32 level = (UInt32)rand.Next(1, 11);
            Decimal price = (Decimal)rand.Next(1000);
            Double exp = (Double)(rand.NextDouble() * 10);
            Double chance = rand.NextDouble();
            Boolean predator = (rand.Next(10) / 2) == 1 ? true : false;
            DateTime date = DateTime.Now.AddDays(-rand.Next(10 * 365));

            /*С# sucks at Random (((
             * Грязный хак - притормаживаем слишком быструю систему, чтобы получился
             * новый сид для рандома. Как вариант - можно использовать методы из криптографических пакетов.
             * */
            System.Threading.Thread.Sleep(1);

            return new Monster(name, level, price, exp, chance, predator, date);
        }

        /// <summary>
        /// Генерация произвольного имени для монстра.
        /// <param name="size">Длина имени</param>
        /// </summary>
        static string RandomName(int size)
        {
            Random _rng = new Random(DateTime.Now.Millisecond);
            const String _chars = "АБВГДЕЁЖЗИКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
            Char[] buffer = new Char[size];

            for (int i = 0; i < size; i++)
            {
                buffer[i] = _chars[_rng.Next(_chars.Length)];
                //if (i == 0) buffer[i] = Char.ToUpper(buffer[i]);
            }
            return new String(buffer);
        }
    }

}
