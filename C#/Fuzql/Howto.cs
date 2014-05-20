using System;
using System.Text;
using Fuzql;

namespace Program
{
    class Program
    {
        static void Main(string[] args)
        {            
            try
            {
                //Init database connection
                Fuzdb db = new Fuzdb("../../tables");
                
                //Init table
                Fuzdb.table Books = new Fuzdb.table();
                //Create or open Books table
                if (!db.exists("Books"))
                {
                    Books = db.create("Books", "Title;String;Date;Int32;Price;Decimal");
                }
                else
                {
                    try
                    {
                        Books = db.open("Books");
                    }
                    catch (Fuzql.TableNotFoundException e)
                    {
                        Console.WriteLine("Error opening table: " + e.Message);
                    }                    
                }
               //Add column
                Books.addColumn("Rate","Int32",0);
                //Selete column
                Books.dropColumn("Date");
                //Add row
                Books.insert("Хоббит;340.50;9");
                
                //Show all table data               
                Books.print(Books.select());
                //Force fuzz data recalc
                Books.FuzzCalc();
                //'WHERE' strings
                String[] WhereArr = new String[] { "Price F= medium", "Rate F= big"};
                //Show strings for given query
                Books.print(Books.select(WhereArr));
                //Update table data
                Books.update(WhereArr, "Price", 500.0);
                //Delete from table where...
                Books.delete(WhereArr);
                
                //Drop table
                db.drop("Books");
                //Close database connection
                db.close();                
            }
            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }           
        }
    }
}
