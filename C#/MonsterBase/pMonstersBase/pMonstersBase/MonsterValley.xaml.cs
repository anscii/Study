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
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Windows.Threading;

namespace pMonstersBase
{
    /// <summary>
    /// Interaction logic for MonsterValley.xaml
    /// </summary>
    public partial class MonsterValley : Window
    {
        Monster M;
        Hero H;
        LinkedList<Monster> mList;
        Boolean allMonstersAreDead;
        Boolean isGameOver;
        //files
        OpenFileDialog ofd = new OpenFileDialog();
        SaveFileDialog sfd = new SaveFileDialog();
        string filefilter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
        //timer
        DispatcherTimer dispatcherTimer;

        public MonsterValley()
        {
            InitializeComponent();
            M = null;
            H = null;
            mList = new LinkedList<Monster>();
            isGameOver = false;
            allMonstersAreDead = false;
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        }

        //------------------------------------------------------------------
        //  Переключение меню в зависимости от текущего таба
        //------------------------------------------------------------------
        
        private void tabMonsters_gotFocus(object sender, RoutedEventArgs e)
        {
            heroMenu.Visibility = Visibility.Hidden;
            gameMenu.Visibility = Visibility.Hidden;
            monsterMenu.Visibility = Visibility.Visible;
        }

        private void tabHero_gotFocus(object sender, RoutedEventArgs e)
        {
            heroMenu.Visibility = Visibility.Visible;
            gameMenu.Visibility = Visibility.Hidden;
            monsterMenu.Visibility = Visibility.Hidden;
        }

        private void tabGame_gotFocus(object sender, RoutedEventArgs e)
        {
            heroMenu.Visibility = Visibility.Hidden;
            gameMenu.Visibility = Visibility.Visible;
            monsterMenu.Visibility = Visibility.Hidden;
        }

        //------------------------------------------------------------------
        //  События в списке монстров
        //------------------------------------------------------------------
        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            ListView lv = sender as ListView;
            if (lv.Items.Count > 0 || allMonstersAreDead) return;

            generateMonsters(10);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            if (lv.SelectedItem == null) return;

            M = lv.SelectedItem as Monster;
            inputId.Text = M.vId;
            inputName.Text = M.vName;
            inputLevel.Text = M.vLevel;
            inputPrice.Text = M.vPrice;
            inputExp.Text = M.vExp;
            inputChance.Text = M.vChance;
            inputPredator.Text = M.vPredator;
            inputDate.Text = M.vDate;
        }

        private void btnSaveMonster_Click(object sender, RoutedEventArgs e)
        {
            ListView lv = monsterListView;
            if (lv.SelectedItem == null) return;

            M = lv.SelectedItem as Monster;

            M.vId = inputId.Text;
            M.vName = inputName.Text;
            M.vLevel = inputLevel.Text;
            M.vPrice = inputPrice.Text;
            M.vExp = inputExp.Text;
            M.vChance = inputChance.Text;
            M.vPredator = inputPredator.Text;
            M.vDate = inputDate.Text;

            monsterListView.Items.Refresh();
        }

        //------------------------------------------------------------------
        // Удаление монстров
        //------------------------------------------------------------------
        private void menuItemClear_Click(object sender, RoutedEventArgs e)
        {
            monsterListView.Items.Clear();
            mList.Clear();
            M.mLastId = 0;
        }

        private void menuItemDelete_Click(object sender, RoutedEventArgs e)
        {       
            M = monsterListView.SelectedItem as Monster;
            if (M == null) return;
            mList.Remove(M);
            monsterListView.Items.Remove(M);
        }

        //------------------------------------------------------------------
        //  Генерация монстров
        //------------------------------------------------------------------
        private void menuItemGenerate10_Click(object sender, RoutedEventArgs e)
        {
            generateMonsters(10);
        }

        private void menuItemGenerate1_Click(object sender, RoutedEventArgs e)
        {
            generateMonsters(1);
        }

        private void generateMonsters(UInt32 n)
        {
            for (int i = 0; i < n; i++)
            {
                M = Monster.random();
                mList.AddLast(M);
                monsterListView.Items.Add(M);
            }
        }

        //------------------------------------------------------------------
        //  Работа с файлами
        //------------------------------------------------------------------
        private void menuLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            ofd.Filter = filefilter;
            bool b = (bool)ofd.ShowDialog(this);
            if (b == false) return;

            string Line;
            string[] strArr;
            char[] charArr = new char[] { ';' };
            try
            {
                FileStream fs = new FileStream(ofd.FileName, FileMode.Open);
                StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                while (sr.EndOfStream != true)  // framework 2.0
                {
                    Line = sr.ReadLine();

                    strArr = Line.Split(charArr);
                   
                    M = new Monster();
                    if (M.mLastId == 0 && (mList.Count > 0)) M.mLastId = Convert.ToUInt32(mList.Last.Value.vId);
                    M.mLastId++;
                  
                    M.vId = M.mLastId.ToString();
                    M.vName = strArr[1].Trim();
                    M.vLevel = strArr[2].Trim();
                    M.vPrice = strArr[3].Trim();
                    M.vExp = strArr[4].Trim();
                    M.vChance = strArr[5].Trim();
                    M.vPredator = strArr[6].Trim();
                    M.vDate = strArr[7].Trim();

                    mList.AddLast(M);
                    monsterListView.Items.Add(M);                 
                }
                sr.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void menuHeroLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            ofd.Filter = filefilter;
            bool b = (bool)ofd.ShowDialog(this);
            if (b == false) return;

            string Line;
            string[] strArr;
            char[] charArr = new char[] { ';' };
            try
            {
                FileStream fs = new FileStream(ofd.FileName, FileMode.Open);
                StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                while (sr.EndOfStream != true)  // framework 2.0
                {
                    Line = sr.ReadLine();

                    strArr = Line.Split(charArr);                                   
                    H = new Hero();

                    H.vName = strArr[0].Trim();
                    H.vClass = strArr[1].Trim();
                    H.vLevel = strArr[2].Trim();
                    H.vBalls = strArr[3].Trim();
                    H.vMoney = strArr[4].Trim();
                    H.vHealth = strArr[5].Trim();
                    H.vExp = strArr[6].Trim();
                    H.vChance = strArr[7].Trim();
                    reloadHero(H);                  
                }
                sr.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void menuSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            if (mList.Count == 0)
            {
                MessageBox.Show("Монстров нет, нечего сохранять", "Ошибка", MessageBoxButton.OK);
                return;
            }
            sfd.Filter = filefilter;          
            bool b = (bool)sfd.ShowDialog(this);
            if (b == false) { return; }
            StreamWriter sw = null;           
            try
            {
                FileStream fsw = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);              
                sw = new StreamWriter(fsw);
                foreach (Monster m in mList)
                {
                    sw.WriteLine(m.ToString());
                }
                sw.Close();
            }
            catch
            {
                MessageBox.Show("Отказ в создании файла", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
        }

        private void menuHeroSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            if (H == null)
            {
                MessageBox.Show("Герой не создан, нечего сохранять", "Ошибка", MessageBoxButton.OK);
                return;
            }
            saveStringToFile(H.ToString());
        }

        private void menuGameSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            if (H == null || H.hGameLog == "")
            {
                MessageBox.Show("Игра еще не произошла, нечего сохранять", "Ошибка", MessageBoxButton.OK);
                return;
            }
            saveStringToFile(H.hGameLog);
        }

        private void saveStringToFile(String value)
        {
            sfd.Filter = filefilter;
            bool b = (bool)sfd.ShowDialog(this);
            if (b == false) { return; }
            try
            {
                System.IO.File.WriteAllText(sfd.FileName, value);
            }
            catch
            {
                MessageBox.Show("При сохранении файла произошла ошибка", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
        }

        //------------------------------------------------------------------
        //  Действия с героем
        //------------------------------------------------------------------
        private void btnSaveHero_Click(object sender, RoutedEventArgs e)
        {
            if (inputHeroName.Text == "" || comboHeroClass.SelectedValue.ToString() == "") return;
            H = Hero.randomByClass(inputHeroName.Text, comboHeroClass.SelectedValue.ToString());
            reloadHero(H);

        }

        private void reloadHero(Hero h)
        {
            heroGrid.DataContext = H;

        }

        //------------------------------------------------------------------
        //  Запуск игры
        //------------------------------------------------------------------
        private void menuItemStartGame_Click(object sender, RoutedEventArgs e)
        {
            if (mList.Count == 0 || H == null)
            {
                MessageBox.Show("Не созданы герой или монстры!");
                return;
            }
            if (H.isDead)
            {
                MessageBox.Show("Герой умер, необходимо создать нового.");
                return;
            }
            tabGame.Visibility = Visibility.Visible;
            tabGame.Focus();
            txtGameLog.DataContext = H;
            H.clearGameLog();
            isGameOver = false;
            H.hGameLog = "Итак, однажды в далекой-далекой галактике " + H.vName
                + " вышел из дома и отважно отправился в Долину Монстров, желая стать настоящим героем и прославить свое имя в легендах.";

            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
            dispatcherTimer.Start();

        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!H.isDead && mList.Count > 0 && !isGameOver)
            {
                startGame();               
            }
            else
            {
                dispatcherTimer.Stop();

                if (mList.Count == 0 && !isGameOver)
                {
                    H.hGameLog = "-------------------------------------";
                    H.hGameLog = "УРА! " + H.vName + " победил всех монстров!";
                    H.hGameLog = "-------------------------------------";
                    allMonstersAreDead = true;
                    M.mLastId = 0;
                }
                if (H.isDead && !isGameOver)
                {
                    H.hGameLog = "-------------------------------------";
                    H.hGameLog = "R.I.P. " + H.vName;
                    H.hGameLog = "Ты был славным героем, но монстры оказались сильнее.";
                    H.hGameLog = "-------------------------------------";
                }
                
                if (!isGameOver) H.hGameLog = "GAME OVER";
                isGameOver = true;
            }
            CommandManager.InvalidateRequerySuggested();
        }

        private void startGame()
        {           
            M = mList.First.Value;
            H.monsterMeet(M);
            mList.RemoveFirst();
            monsterListView.Items.RemoveAt(0);
        }
    }
}
