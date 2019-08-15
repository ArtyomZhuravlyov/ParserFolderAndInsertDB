using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParserFolder
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// имя выбранной папки 
        /// </summary>
        string _NameFolder { get; set; } 
        string _Catalog; //хранит тлько имя папки без пути
        string _Catalog2; //хранит тлько имя папки без пути
        string _Catalog3; //хранит тлько имя папки без пути
        string _Catalog4; //хранит тлько имя папки без пути

        /// <summary>
        /// Имя файла с раширением 
        /// </summary>
        string _NameFile { get; set; }

        /// <summary>
        /// Описание 
        /// </summary>
        string _Description { get; set; }

        /// <summary>
        /// Путь описания
        /// </summary>
        string DecriptionPath { get; set; }

        /// <summary>
        /// Имя файла без раширения 
        /// </summary>
        string _NameFileWithoutException { get; set; }

        /// <summary>
        /// conection with DB 
        /// </summary>
        private string connectionString;

        /// <summary>
        /// Считает глубину погружения по каталогам (0 уровень папка выбранная при перетаскивании)
        /// </summary>
        private int countDepp = 0;

        /// <summary>
        /// Записывает папки в которых сейчас гуляет (по сути принадлежащие категории для файла)
        /// </summary>
        private List<string> ListCategory = new List<string>();
        
        public Form1()
        {
            InitializeComponent();
            // получаем строку подключения
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            var f = (string[])(e.Data.GetData(DataFormats.FileDrop));
            if (f.Count() == 1)
                _NameFolder = f[0];
            else MessageBox.Show("Выбрано больше одного элемента");
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                e.Effect = DragDropEffects.All;
            }
        }
        /// <summary>
        /// Выбор папки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog()) 
                if (dialog.ShowDialog() == DialogResult.OK)
                    _NameFolder = dialog.SelectedPath;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(_NameFolder))
                _NameFolder = @"C:\Users\79504\Desktop\Каталог для магазина";
           // DirectoryInfo dirInfo = new DirectoryInfo(NameFolder);
            var listFolder = Directory.GetDirectories(_NameFolder);



            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("DELETE FROM " + "Products", connection)) //очищаем таблицу
                {
                    command.ExecuteNonQuery();
                }

                //смотрим есть ли файлы в текущей папке, если нет, то спускаемся в папки ниже
                //if ((Directory.GetFiles(_NameFolder)).Count() > 0)
                formAdressAndDescription(_NameFolder, connection);
                //не универсальая вещь
                //foreach (var _listFolder2 in listFolder) //смотрит папки в первой папке //1 уровень
                //{
                //    _Catalog = GetCatalogName(_listFolder2);
                //    var listFolderTwoLevel = Directory.GetDirectories(_listFolder2);


                    //    foreach (var listFolderTwoLevel2 in listFolderTwoLevel)// смотрит папки //2 уровень
                    //    {
                    //        _Catalog2 = GetCatalogName(listFolderTwoLevel2);
                    //        var listFile = Directory.GetFiles(listFolderTwoLevel2);
                    //       // if(listFile.Count == 0 && )
                    //        foreach (var listFile2 in listFile) //смотрит файлы
                    //        {
                    //            _NameFile = GetFileName(listFile2); //имя файла для Адреса изображения с раширеним
                    //            if (_NameFile.IndexOf(".png") > -1 || _NameFile.IndexOf(".jpg") > -1) //так как там лежат ещё и тхт их надо исключить
                    //            {
                    //                _NameFileWithoutException = Path.GetFileNameWithoutExtension(listFile2); //имя файла для столбца  - название 

                    //                DecriptionPath = listFolderTwoLevel2 + "\\" + _NameFileWithoutException + ".txt";
                    //                _Description = ReadDescription(DecriptionPath);//ФОРМИРОВАНИЕ ОПИСАНИЯ ИЗ ТХТ
                    //                InsertDB(connection);
                    //            }
                    //        }
                    //    }
                    //}
                connection.Close();
            }
        }
        /// <summary>
        /// Возвращает только имя каталога
        /// </summary>
        /// <returns></returns>
        private string GetCatalogName(string catalog)
        {
            int count = catalog.LastIndexOf("\\") + 1; //так как иначе слеши не обрезаются
            catalog = catalog.Substring(count);

            return catalog;
        }

        /// <summary>
        /// Возвращает только имя файла с расширением
        /// </summary>
        /// <returns></returns>
        private string GetFileName(string File)
        {
            int count = File.LastIndexOf("\\") + 1; //так как иначе слеши не обрезаются
            File = File.Substring(count);

            return File;
        }

        private void InsertDB(SqlConnection connectionTemp)
        { //Решил для безопасности использовать параметры
           // string sqlExpression = "INSERT INTO Products (Name, Description, Address, Category, Category2, Price) VALUES ('" + _NameFileWithoutException + "', '" + _Description +"', '" + _NameFile + "', '"+ _Catalog + "', '" + _Catalog2 + "', "+ "1" + ")";
            string sqlExpression = "INSERT INTO Products (Name, Description, Address, Category, Category2, Price) VALUES (@Name, @Description, @Address, @Category, @Category2, @Price)";

            SqlCommand command = new SqlCommand(sqlExpression, connectionTemp);

            // создаем параметр для имени
            SqlParameter nameParam = new SqlParameter("@Name", _NameFileWithoutException);
            command.Parameters.Add(nameParam);
  
            SqlParameter DescriptionParam = new SqlParameter("@Description", _Description);
            // добавляем параметр к команде
            command.Parameters.Add(DescriptionParam);

            SqlParameter AddressParam = new SqlParameter("@Address", _NameFile);
            // добавляем параметр к команде
            command.Parameters.Add(AddressParam);

            SqlParameter CategoryParam = new SqlParameter("@Category", ListCategory[0]); //, _Catalog);
            // добавляем параметр к команде
            command.Parameters.Add(CategoryParam);

            SqlParameter Category2Param;
            if (ListCategory.Count() > 1)
                Category2Param = new SqlParameter("@Category2", ListCategory[1]); // _Catalog2);
            else
                Category2Param = new SqlParameter("@Category2", "");                                                                                   // добавляем параметр к команде
            command.Parameters.Add(Category2Param);
            

            SqlParameter PriceParam = new SqlParameter("@Price", 1);
            // добавляем параметр к команде
            command.Parameters.Add(PriceParam);

            int number = command.ExecuteNonQuery();
            
        }

        /// <summary>
        /// Считывает ТХТ фаил в котором лежит описание продукта
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string ReadDescription(string path)
        {
            string text;
            using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))
            {
                text = sr.ReadToEnd();
            }
            return text;
        }

        /// <summary>
        /// Нужно ли считать как категорию первую корневую папку
        /// </summary>
        private bool AddFolder = false;

        private void formAdressAndDescription(string PathFolder, SqlConnection connectionTemp )
        {
            var listFile = Directory.GetFiles(PathFolder);
            var listFolder = Directory.GetDirectories(PathFolder);

            if (AddFolder)
                ListCategory.Add(GetCatalogName(PathFolder)); //добавляем категорию чтобы у файла были все категории и удаляем как прошли 
            else AddFolder = true;

            if (listFile.Count()>0)
            {
               
                foreach (var listFile2 in listFile) //смотрит файлы
                {
                    _NameFile = GetFileName(listFile2); //имя файла для Адреса изображения с раширеним
                    if (_NameFile.IndexOf(".png") > -1 || _NameFile.IndexOf(".jpg") > -1) //так как там лежат ещё и тхт их надо исключить
                    {
                        _NameFileWithoutException = Path.GetFileNameWithoutExtension(listFile2); //имя файла для столбца  - название 

                        DecriptionPath = PathFolder + "\\" + _NameFileWithoutException + ".txt";
                        _Description = ReadDescription(DecriptionPath);//ФОРМИРОВАНИЕ ОПИСАНИЯ ИЗ ТХТ
                        InsertDB(connectionTemp);
                    }
                }
                
            }

            if (listFolder.Count() > 0)
            {
                foreach (string Folder in listFolder)
                {
                    formAdressAndDescription(Folder, connectionTemp);
                }
            }
            ListCategory.Remove(GetCatalogName(PathFolder));
        }


    }
}
