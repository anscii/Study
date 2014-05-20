/*
 * Copyright © 2013 Nataly Akentyeva. All rights reserved.

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
using System.Linq;
using System.Linq.Expressions;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;

namespace Fuzql
{
    public class Fuzdb
    {
        protected String dbFolder = "";
        protected String tableExt = ".fzt";
        protected String hashExt = ".fzx";
        protected String fuzzHeaderExt = ".fzh";
        protected String fuzzDataExt = ".fzd";
        protected List<table> openedTables = new List<table>();        

        public Fuzdb()
            : this(".")
        {
        }

        public Fuzdb(String dbFolder)
        {
            if (Directory.Exists(dbFolder))
            {
                this.dbFolder = dbFolder;
                Directory.SetCurrentDirectory(dbFolder);
                bool IsExists = Directory.Exists("data");

                if (!IsExists)
                    Directory.CreateDirectory("data");
            }
            else
            {                
                throw new System.IO.DirectoryNotFoundException("Database directory not found");             
            }
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }

        ~Fuzdb()
        {
            this.close();
        }

        public override String ToString()
        {
            return this.dbFolder;
        }

        public table create(String tableName)
        {
            String fileName = tableName + this.tableExt;
            if (!File.Exists(fileName))
            {
                File.CreateText(fileName);
                table newTable = new table();
                this.openedTables.Add(newTable);
                return newTable;
            }
            else
            {
                throw new FileLoadException("Table " + tableName + " already exists");
            }
        }

        public table create(String tableName, String headersList)
        {
            String fileName = tableName + this.tableExt;
            if (!File.Exists(fileName))
            {
                using (StreamWriter sw = File.CreateText(fileName))
                {
                    sw.Write(headersList);
                }

                table newTable = new table(tableName);
                this.openedTables.Add(newTable);

                return newTable;
            }
            else
            {
                throw new FileLoadException("Table " + tableName + " already exists");
            }
        }

        public Boolean exists(String tableName)
        {
            return File.Exists(tableName + this.tableExt);
        }

        public table open(String tableName)
        {            
            table newTable = new table(tableName);
            this.openedTables.Add(newTable);
            return newTable;
        }

        public void drop(String tableName)
        {
            
            table dropTable = this.openedTables.Find(x => x.tableName == tableName);
            
            if (dropTable == null)
            {
                dropTable = new table(tableName, false);                
            }
            else
            {
                Boolean result = this.openedTables.Remove(dropTable);                
            }                      
            dropTable.drop();           
        }

        public void flush()
        {            
            foreach (table t in this.openedTables)
            {               
               t.flush();
            }
        }

        public void close()
        {
            this.flush();
            this.openedTables.Clear();
            
        }

        public static int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
        
        public class table : Fuzdb
        {
            public String tableName;
            private String tableFileName;
            private String hashFileName;
            private String fuzzHeaderFileName;
            private String fuzzDataFileName;

            public List<Header> headers = new List<Header>();
            public List<Row> body = new List<Row>();
            public char[] delim = new char[] { ';' };
            private String hash = "";

            public table()
            {
            }

            public table(String tableName) : this(tableName, true)
            {                
            }

            public table(String tableName, Boolean loadData)
            {
                this.tableName = tableName;
                this.tableFileName = tableName + base.tableExt;
                this.hashFileName = "data/" + tableName + base.hashExt;
                this.fuzzHeaderFileName = "data/" + tableName + base.fuzzHeaderExt;
                this.fuzzDataFileName = "data/" + tableName + base.fuzzDataExt;
                
                if (loadData)
                {
                    this.loadTable();

                    this.hash = this.GetHash();
                    if (this.isHashChanged())
                    {
                        this.FuzzCalc();                        
                    }
                    else
                    {
                        this.loadFuzzDataFromFile();                        
                    }
                }
            }

            public String GetHash()
            {
                String hashStr = "";
                foreach (Header header in this.headers)
                {
                    hashStr += header.title + header.type;
                }

                foreach (Row row in this.body)
                {
                    foreach (Header header in this.headers)
                    {
                        hashStr += row.cells[header.title];
                    }
                }
                byte[] bytes = Encoding.Unicode.GetBytes(hashStr);
                byte[] hash = MD5.Create().ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "");
            }

            private Boolean isHashChanged()
            {                
                return this.GetHash() != this.getSavedHash();
            }

            private String getSavedHash()
            {
                try
                {                    
                   return File.ReadAllText(this.hashFileName);                   
                }
                catch (FileNotFoundException e)
                {
                    this.createHashFile();
                    return "";
                }
            }

            private void createHashFile()
            {
                if (!File.Exists(this.tableName + base.hashExt))
                {
                    FileStream f = File.Create(this.hashFileName);                   
                    f.Close();
                }
            }            
            
            private void saveHashToFile()
            {
               String newHash = this.GetHash();
                
               File.WriteAllText(this.hashFileName, newHash.ToString());
               this.hash = newHash;
               
            }

            public void FuzzCalc()
            {
                if (this.body.Count < 2) return;

                foreach (Header header in this.headers)
                {
                    Object[] limits = new Object[4]{0,0,0,0};
                 
                    switch (header.type)
                    {
                        case "String": break;
                        case "Char": break;
                        case "Int32":
                            limits[3] = (Int32) this.body.Max(t => Convert.ToInt32(t.cells[header.title]));
                            limits[0] = (Int32)this.body.Min(t => Convert.ToInt32(t.cells[header.title]));
                            limits[1] = (Int32) limits[0] + ((Int32) limits[3] - (Int32) limits[0]) / 3;
                            limits[2] = (Int32) limits[3] - ((Int32) limits[3] - (Int32) limits[0]) / 3;
                            break;
                        case "Double":
                            limits[3] = (Double)this.body.Max(t => Convert.ToDouble(t.cells[header.title]));
                            limits[0] = (Double)this.body.Min(t => Convert.ToDouble(t.cells[header.title]));
                            limits[1] = (Double) limits[0] + ((Double) limits[3] - (Double) limits[0]) / 3;
                            limits[2] = (Double) limits[3] - ((Double) limits[3] - (Double) limits[0]) / 3;
                            break;
                        case "Decimal":
                            limits[3] = (Decimal) this.body.Max(t => Convert.ToDecimal(t.cells[header.title]));
                            limits[0] = (Decimal) this.body.Min(t => Convert.ToDecimal(t.cells[header.title]));
                            limits[1] = (Decimal) limits[0] + ((Decimal) limits[3] - (Decimal) limits[0]) / 3;
                            limits[2] = (Decimal) limits[3] - ((Decimal) limits[3] - (Decimal) limits[0]) / 3;
                            break;

                        default: Console.WriteLine("\nIllegal data type in column {0} "); break;                          
                    }

                    header.fuzzDelta = Math.Ceiling((Convert.ToDouble(limits[1]) - Convert.ToDouble(limits[0])) * 0.2); //@todo calc it!

                    switch (header.type)
                    {
                        case "String": this.fuzzCalcColumn(header); break;
                        case "Char": this.fuzzCalcColumn(header); break;
                        case "Int32":                          
                            for (int i = 0; i < 3; i++)
                            {
                                header.fuzzLimits[i]["a"] = Convert.ToInt32(limits[i]) - Convert.ToInt32(header.fuzzDelta);
                                header.fuzzLimits[i]["b"] = Convert.ToInt32(limits[i]) + Convert.ToInt32(header.fuzzDelta);
                                header.fuzzLimits[i]["c"] = Convert.ToInt32(limits[i+1]) - Convert.ToInt32(header.fuzzDelta);
                                header.fuzzLimits[i]["d"] = Convert.ToInt32(limits[i+1]) + Convert.ToInt32(header.fuzzDelta);
                                header.fuzzLimits[i]["b_a"] = Convert.ToInt32(header.fuzzLimits[i]["b"]) - Convert.ToInt32(header.fuzzLimits[i]["a"]);
                                header.fuzzLimits[i]["d_c"] = Convert.ToInt32(header.fuzzLimits[i]["d"]) - Convert.ToInt32(header.fuzzLimits[i]["c"]);
                            }
                            this.fuzzCalcColumn(header);                                
                            break;
                        case "Double":                            
                            for (int i = 0; i < 3; i++)
                            {
                                header.fuzzLimits[i]["a"] = Convert.ToDouble(limits[i]) - Convert.ToDouble(header.fuzzDelta);
                                header.fuzzLimits[i]["b"] = Convert.ToDouble(limits[i]) + Convert.ToDouble(header.fuzzDelta);
                                header.fuzzLimits[i]["c"] = Convert.ToDouble(limits[i+1]) - Convert.ToDouble(header.fuzzDelta);
                                header.fuzzLimits[i]["d"] = Convert.ToDouble(limits[i+1]) + Convert.ToDouble(header.fuzzDelta);
                                header.fuzzLimits[i]["b_a"] = Convert.ToDouble(header.fuzzLimits[i]["b"]) - Convert.ToDouble(header.fuzzLimits[i]["a"]);
                                header.fuzzLimits[i]["d_c"] = Convert.ToDouble(header.fuzzLimits[i]["d"]) - Convert.ToDouble(header.fuzzLimits[i]["c"]);
                            }
                             this.fuzzCalcColumn(header);
                             break;
                        case "Decimal":                             
                             for (int i = 0; i < 3; i++)
                             {                                
                                 header.fuzzLimits[i]["a"] = Convert.ToDecimal(limits[i]) - Convert.ToDecimal(header.fuzzDelta);
                                 header.fuzzLimits[i]["b"] = Convert.ToDecimal(limits[i]) + Convert.ToDecimal(header.fuzzDelta);
                                 header.fuzzLimits[i]["c"] = Convert.ToDecimal(limits[i + 1]) - Convert.ToDecimal(header.fuzzDelta);
                                 header.fuzzLimits[i]["d"] = Convert.ToDecimal(limits[i + 1]) + Convert.ToDecimal(header.fuzzDelta);
                                 header.fuzzLimits[i]["b_a"] = Convert.ToDecimal(header.fuzzLimits[i]["b"]) - Convert.ToDecimal(header.fuzzLimits[i]["a"]);
                                 header.fuzzLimits[i]["d_c"] = Convert.ToDecimal(header.fuzzLimits[i]["d"]) - Convert.ToDecimal(header.fuzzLimits[i]["c"]);
                             }
                             this.fuzzCalcColumn(header);
                             break;
                        default: Console.WriteLine("\nIllegal data type in column {0} "); break;                          
                    }                  
                }
            }

            private void fuzzCalcColumn(Header header)
            {                
                    foreach (Row row in this.body)
                    {
                        for (Int32 i=0;i<3;i++)
                        {
                            Double val = 0;
                            try
                            {
                                if (Convert.ToDouble(row.cells[header.title]) >= Convert.ToDouble(header.fuzzLimits[i]["a"])
                                    && Convert.ToDouble(row.cells[header.title]) < Convert.ToDouble(header.fuzzLimits[i]["b"]))
                                {
                                    val = 1 - (Convert.ToDouble(header.fuzzLimits[i]["b"]) - Convert.ToDouble(row.cells[header.title])) / Convert.ToDouble(header.fuzzLimits[i]["b_a"]);
                                }
                                else if (Convert.ToDouble(row.cells[header.title]) >= Convert.ToDouble(header.fuzzLimits[i]["b"])
                                    && Convert.ToDouble(row.cells[header.title]) <= Convert.ToDouble(header.fuzzLimits[i]["c"]))
                                {
                                    val = 1;
                                }
                                else if (Convert.ToDouble(row.cells[header.title]) > Convert.ToDouble(header.fuzzLimits[i]["c"])
                                    && Convert.ToDouble(row.cells[header.title]) <= Convert.ToDouble(header.fuzzLimits[i]["d"]))
                                {
                                    val = 1 - (Convert.ToDouble(row.cells[header.title]) - Convert.ToDouble(header.fuzzLimits[i]["c"])) / Convert.ToDouble(header.fuzzLimits[i]["d_c"]);
                                }
                                else
                                {
                                    val = 0;
                                }
                            }
                            catch (FormatException e)
                            {
                                //Console.WriteLine("FuzzCalcColumn " + header.title + ". Error: " + e.Message);
                            }
                            
                            
                            if (row.fuzzData.ContainsKey(header.title + "_" + i))
                            {
                                row.fuzzData[header.title + "_" + i] = val;
                            }
                            else
                            {
                                row.fuzzData.Add(header.title + "_" + i, val);
                            }                           
                        }                        
                    }
            }

            public class Header
            {
                public String title;
                public String type;
                public Dictionary<String, Object>[] fuzzLimits = new Dictionary<String, Object>[3];
                public Object fuzzDelta;
 
                public Header(String title, String type)
                {
                    this.title = title;
                    this.type = type;
                    for (Int32 i = 0; i < 3; i++)
                    {
                        Dictionary<String, Object> limits = new Dictionary<string, object>{{ "a", 0}, {"b", 0},{ "c", 0}, {"d", 0},
                                                                                     { "b_a", 0}, {"d_c", 0}};
                        this.fuzzLimits[i] = limits;                        
                    }
                }
            }

            public class Row
            {
                public Dictionary<String, Object> cells;
                public Dictionary<String, Double> fuzzData;

                public Row()
                {
                    this.cells = new Dictionary<string, object>();
                    this.fuzzData = new Dictionary<String, Double> { };
                }

                public bool Equal(String title, String val)
                {
                    return this.cells[title].ToString().Equals(val);
                }

                public bool Equal(String title, Int32 val)
                {
                    return this.cells[title].Equals(val);
                }

                public int Compare(String title, String val)
                {
                    return this.cells[title].ToString().CompareTo(val);
                }

                public int Compare(String title, Int32 val)
                {

                    Int32 curVal = Convert.ToInt32(this.cells[title]);

                    return val.CompareTo(curVal);
                }
                
                public bool Contain(String title, String val)
                {
                    return this.cells[title].ToString().Contains(val);
                }

                public bool FuzzEqual(String title, String val)
                {
                    Int32 index;
                    switch (val.ToLower())
                    {
                        case "small":
                            index = 0;
                            break;
                        case "medium":
                            index = 1;
                            break;
                        case "big":
                            index = 2;
                            break;
                        default:
                            Int32 counts = 2;
                            return Fuzdb.LevenshteinDistance(this.cells[title].ToString(), val) <= counts;                            
                    }
                    Double threshold = 0.5;
                    return this.fuzzData[title +"_" + index] >= threshold;                    
                }

                public bool FuzzEqual(String title, Int32 val, int threshold)
                {
                    Console.WriteLine("@todo FuzzEqual for Int32");
                    return false;
                }

                public bool FuzzCompare(String title, Int32 val, float threshold)
                {
                    Console.WriteLine("@todo FuzzCompare for Int32");
                    return false;
                }
            }

            protected void loadTable()
            {
                string strLine;
                string[] strArray;
                string[] headerArray;
              
                try
                {
                    FileStream tableFile = new FileStream(this.tableFileName, FileMode.Open);
                    StreamReader sr = new StreamReader(tableFile);
                    //Get headers from first row
                    strLine = sr.ReadLine();
                    headerArray = strLine.Split(delim);

                    for (int x = 0; x <= headerArray.GetUpperBound(0); x += 2)
                    {
                        Header header = new Header(headerArray[x], headerArray[x + 1]);
                        this.headers.Add(header);
                    }

                    while ((strLine = sr.ReadLine()) != null)
                    {
                        //Make Dictionary<string,string> dataRow
                        //and add it ro the List<Dictionary<string,string>> data
                        strArray = strLine.Split(delim);

                        Row dataRow = new Row();

                        for (int x = 0; x <= strArray.GetUpperBound(0); x++)
                        {
                            object val = strArray[x];

                            switch (this.headers[x].type)
                            {
                                case "Int32": val = Convert.ToInt32(val); break;
                                case "String": break;
                                case "Char": break;
                                case "Double": val = Convert.ToDouble(val); Console.WriteLine("{0:.00}", val); break;
                                case "Decimal": val = Convert.ToDecimal(val); break;

                                default: Console.WriteLine("\nНедопустимый тип данных в столбце{0} ",
                                   this.headers[x].title); val = null; break;
                            }
                            
                            dataRow.cells.Add(this.headers[x].title, strArray[x]);                            
                        }

                        body.Add(dataRow);
                    }

                    sr.Close();
                }
                catch (IOException ex)
                {
                    throw new TableNotFoundException(this.tableName);
                }
            }

            public new void flush()
            {                                                
                if (this.isHashChanged())
                {
                    this.saveTableToFile();
                    this.saveFuzzDataToFile();
                    this.saveHashToFile();
                }
            }

            private void saveTableToFile()
            {
                using (FileStream fs = File.Open(this.tableFileName, FileMode.Truncate, FileAccess.Write))
                {
                    String headersList = "";

                    foreach (Header header in this.headers)
                    {
                        headersList += header.title + delim[0];
                        headersList += header.type + delim[0];
                    }

                    headersList = headersList.TrimEnd(delim[0]);

                    StreamWriter sw = new StreamWriter(fs);

                    sw.WriteLine(headersList);
                    sw.Flush();

                    foreach (Row row in this.body)
                    {
                        String rowStr = "";

                        foreach (Object val in row.cells.Values)
                        {
                            rowStr += val.ToString() + delim[0];
                        }

                        rowStr = rowStr.TrimEnd(delim[0]);

                        sw.WriteLine(rowStr);
                    }
                    sw.Close();
                }
            }

            private void saveFuzzDataToFile()
            {
                if (this.body.Count < 2) return;
                //Saving fuzz limits data from headers
                using (FileStream fs = File.Open(this.fuzzHeaderFileName, FileMode.Create, FileAccess.Write))
                {
                    StreamWriter sw = new StreamWriter(fs);
                    foreach (Header header in this.headers)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            String rowStr = header.title + "_" + i + delim[0];
                           
                            foreach (Object val in header.fuzzLimits[i].Values)
                            {                                
                                rowStr += val.ToString() + delim[0];
                            }

                            rowStr = rowStr.TrimEnd(delim[0]);

                            sw.WriteLine(rowStr);
                        }                        
                    }
                    sw.Close();
                }

                //Saving fuzz rows data
                using (FileStream fs = File.Open(this.fuzzDataFileName, FileMode.Create, FileAccess.Write))
                {
                    StreamWriter sw = new StreamWriter(fs);

                    foreach (Row row in this.body)
                    {
                        String rowStr = "";                      

                        foreach (Header header in this.headers)
                        {                            
                            for (int i=0; i < 3; i++)
                            {
                                String title = header.title + "_"+i;
                                
                                if (row.fuzzData.ContainsKey(title))
                                {
                                    rowStr += title + delim[0] + row.fuzzData[title] + delim[0];
                                }
                            }
                        }
                        rowStr = rowStr.TrimEnd(delim[0]);
                        sw.WriteLine(rowStr);
                    }
                    sw.Close();                   
                }
            }

            private void loadFuzzDataFromFile()
            {
                if (this.body.Count < 2) return;
                string strLine;
                string[] strArray;
                
                try
                {
                    //read fuzz limits from 
                    FileStream tableFile = new FileStream(this.fuzzHeaderFileName, FileMode.Open);
                    StreamReader sr = new StreamReader(tableFile);
    
                    while ((strLine = sr.ReadLine()) != null)//чтение строк данных из файла
                    {
                        //Make Dictionary<string,string> dataRow
                        //and add it to the List<Dictionary<string,string>> data
                        char[] charArray = new char[] { '_', delim[0] };
                        strArray = strLine.Split(charArray);
                        
                        Header header = this.headers.Find(x => x.title == strArray[0]);
                        Dictionary<String, Int32> limits = new Dictionary<string, Int32> {{ "a", 2}, {"b", 3},{ "c", 4}, {"d", 5},
                                                                                 { "b_a", 6}, {"d_c", 7}};
                        foreach (KeyValuePair<String, Int32> item in limits)
                        {
                            header.fuzzLimits[Convert.ToInt32(strArray[1])][item.Key] = strArray[item.Value];
                        }                            
                        
                    }
                    sr.Close();
                }                
                catch (IOException e)
                {                                
                    Console.WriteLine("Fuzz limits file is corrupted. Error: " + e.Message);
                }

                try
                {
                    //read fuzz data
                    FileStream tableFile = new FileStream(this.fuzzDataFileName, FileMode.Open);
                    StreamReader sr = new StreamReader(tableFile);
                    Int32 i = 0;

                    while ((strLine = sr.ReadLine()) != null) //read rows from file
                    {                      
                        strArray = strLine.Split(delim, StringSplitOptions.RemoveEmptyEntries);
                        
                        Dictionary<String, Double> dataArr = new Dictionary<string, double> { };

                        for (int x = 0; x <= strArray.GetUpperBound(0); x += 2)
                        {
                            dataArr.Add(strArray[x], Convert.ToDouble(strArray[x+1]));
                        }
                        
                        Row row = this.body.ElementAt(i);
                        foreach (KeyValuePair<String, Double> item in dataArr)
                        {
                            row.fuzzData.Add(item.Key, item.Value);
                        }
                       
                        i++;
                     }
                    
                    sr.Close();
                }                
                catch (IOException e)
                {                               
                    Console.WriteLine("Fuzz limits file is corrupted. Error: " + e.Message);
                }
            }

            public void addColumn(String title, String type, object defaultValue)
            {

                this.headers.Add(new Header(title, type));

                foreach (Row row in this.body)
                {
                    row.cells.Add(title, defaultValue);
                }
            }
            
            public void dropColumn(String title)
            {
                Header column = this.headers.Find(x=>x.title == title);
                
                if (column != null)
                {                    
                    this.headers.Remove(column);
                    
                    foreach (Row row in this.body)
                    {
                        row.cells.Remove(column.title);
                        for (Int32 i = 0; i < 3; i++)
                        {
                            if (row.fuzzData.ContainsKey(column.title + "_" + i))
                            {
                                row.fuzzData.Remove(column.title + "_" + i);
                            }
                        }                            
                    }
                    this.saveTableToFile();
                }            
            }

            public void insert(String row)
            {
                string[] strArray = row.Split(delim);

                Row dataRow = new Row();

                for (int x = 0; x <= strArray.GetUpperBound(0); x++)
                {
                    dataRow.cells.Add(this.headers[x].title, strArray[x]);
                }

                this.body.Add(dataRow);
                this.FuzzCalc();
            }

            public void delete(String[] whereArr)
            {                
                Func<Row, bool> pr = this.wherePredicate(whereArr);

                IEnumerable<Row> matches = this.body.Where(pr);

                var setToRemove = new HashSet<Row>(matches);
                if (setToRemove.Count > 0)
                {
                    this.body.RemoveAll(x => setToRemove.Contains(x));
                    this.FuzzCalc();
                }
            }

            public void drop()
            {
                File.Delete(this.tableFileName);
                File.Delete(this.hashFileName);
                File.Delete(this.fuzzHeaderFileName);
                File.Delete(this.fuzzDataFileName);
            }

            public IEnumerable<Row> select()
            {
                int limit = this.body.Count;

                return this.select(limit);
            }

            public IEnumerable<Row> select(int limit)
            {
                IEnumerable<Row> matches;

                matches = (from item in this.body
                           select item
                           ).Take(limit);
                return matches;
            }
            
            public IEnumerable<Row> select(String[] whereArr)
            {
                int limit = this.body.Count;
                
                return this.select(whereArr, limit);
            }

            public IEnumerable<Row> select(String[] whereArr, int limit)
            {
                Func<Row, bool> pr = this.wherePredicate(whereArr);
                return this.body.Where(pr).Take(limit);
            }

            public void update(String[] whereArr, String column, Object value)
            {
                if (!this.headers.Exists(x => x.title == column))
                {
                    throw new ColumnNotFoundException(column);
                }

                Func<Row, bool> pr = this.wherePredicate(whereArr);

                IEnumerable<Row> matches = this.body.Where(pr);

                var setToUpdate = new HashSet<Row>(matches);
                if (setToUpdate.Count > 0)
                {
                    foreach (Row row in setToUpdate)
                    {
                        row.cells[column] = value;
                    }                     
                    this.FuzzCalc();
                }
            }

            private Func<Row, bool> wherePredicate(String[] whereArr)
            {
                Func<Row, bool>[] where = new Func<Row,bool>[whereArr.Length];
                int i = 0;
                
                foreach (String cond in whereArr)
                {                    
                    String op = "=";                    

                    if (cond.Contains(">="))
                    {
                        op = ">=";
                    }
                    else if (cond.Contains("<="))
                    {
                        op = "<=";
                    }
                    else if (cond.Contains("~="))
                    {
                        op = "~=";
                    }
                    else if (cond.Contains("F="))
                    {
                        op = "F=";
                    }
                    else if (cond.Contains("F>"))
                    {
                        op = "F>";
                    }
                    else if (cond.Contains("F<"))
                    {
                        op = "F<";
                    }
                    else if (cond.Contains("<"))
                    {
                        op = "<";
                    }
                    else if (cond.Contains(">"))
                    {
                        op = ">";
                    }
                    else if (cond.Contains("="))
                    {
                        op = "=";
                    }
                                        
                    String[] mArr = cond.Split(new string[] { op }, StringSplitOptions.None);
                    for (Int32 k = 0; k <= 1;k++ )
                    {
                        mArr[k] = mArr[k].Trim();                       
                    }
                    
                    switch (op)
                    {                            
                        case ">=":
                            where[i] = (item => item.Compare(mArr[0], mArr[1]) == 1
                                                    || item.Equal(mArr[0], mArr[1]));
                            break;
                        case ">":
                            where[i] = (item => item.Compare(mArr[0], mArr[1]) == 1);
                            break;
                        case "<=":
                            where[i] = (item => item.Compare(mArr[0], mArr[1]) == -1
                                                || item.Equal(mArr[0], mArr[1]));
                            break;
                        case "<":
                            where[i] = (item => item.Compare(mArr[0], mArr[1]) == -1);
                            break;
                        case "~=":
                            where[i] = (item => item.Contain(mArr[0], mArr[1]));
                            break;
                        case "F=":
                            where[i] = (item => item.FuzzEqual(mArr[0], mArr[1]));
                            break;                       
                        case "=":
                        default:
                            where[i] = (item => item.Equal(mArr[0], mArr[1]));
                            break;                          
                    }
                    i++;
                }
                Func<Row, bool> whereAll = And<Row>(where);
                return whereAll;
            }

            public static Func<Row, bool> And<Row>(params Func<Row, bool>[] predicates)
            {
                return t => predicates.All(predicate => predicate(t));
            }

            public static Func<Row, bool> Or<Row>(params Func<Row, bool>[] predicates)
            {
                return t => predicates.Any(predicate => predicate(t));
            }

            public void print(IEnumerable<Row> items)
            {
                Console.WriteLine("\n\t\t\tTable: {0,-20}\n", this.tableName);

                foreach (Header header in this.headers)
                {
                    Console.Write("[ {0,-13}]", header.title);
                }
                Console.WriteLine();
                foreach (var m in items)
                {
                    foreach (var val in m.cells)
                    {
                        Console.Write(" {0,-14}", val.Value);
                    }
                    Console.WriteLine();
                }
            }
        }        
    }

    public class TableNotFoundException : Exception
    {
        private string tableName;

        public string TableName { get { return tableName; } }
        public override string Message
        {
            get
            {
                if (tableName == null) return base.Message;
                else
                    return "Table " + TableName + " does not exists or no access to the table file.";
            }
        }

        public TableNotFoundException() : base() { }        
        public TableNotFoundException(string message, Exception innerException) : base(message, innerException) { }             
        public TableNotFoundException(string fileName) : this() { this.tableName = fileName; } 
    }

    public class ColumnNotFoundException : Exception
    {
        private string columnName;
        
        public string ColumnName { get { return columnName; } }
        public override string Message
        {
            get
            {
                if (columnName == null) return base.Message;
                else
                    return "Column " + ColumnName + " not found.";
            }
        }
        
        public ColumnNotFoundException() : base() { }
        public ColumnNotFoundException(string message, Exception innerException) : base(message, innerException) { }        
        public ColumnNotFoundException(string column) : this() { this.columnName = column; }
    }
}

