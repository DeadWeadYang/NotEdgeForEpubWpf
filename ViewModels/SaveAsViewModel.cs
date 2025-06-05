using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using NotEdgeForEpubWpf.Models;
using NotEdgeForEpubWpf.Models.AnnotationModel;
using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using VersOne.Epub;

namespace NotEdgeForEpubWpf.ViewModels
{
    public partial class SaveAsViewModel:ObservableObject
    {
        enum SaveAsMode:int
        {
            PackIntoEPUB=0,
            OnlyAnnotation=1,
            OnlyEPUB=2
        }
        [ObservableProperty]
        private int saveChoice=0;
        public readonly BookModel bookModel;
        public readonly EpubBookRef bookRef;
        public readonly IDocumentStore dataBase;
        public SaveAsViewModel(BookModel bookModel,EpubBookRef bookRef,IDocumentStore dataBase)
        {
            this.bookModel = bookModel;
            this.bookRef= bookRef;
            this.dataBase = dataBase;
        }
        [RelayCommand]
        public void  TrySaveAs()
        {
            switch (SaveChoice)
            {
                case (int)SaveAsMode.PackIntoEPUB:
                    {
                        SaveAsPackIntoEPUB();
                        break;
                    }
                case (int)SaveAsMode.OnlyAnnotation:
                    {
                        SaveAsOnlyAnnotation();
                        break;
                    }
                case (int)SaveAsMode.OnlyEPUB:
                    {
                        SaveAsOnlyEPUB();
                        break;
                    }
                default:break;
            }
        }
        public void SaveAsOnlyEPUB()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = bookRef.Title,       // Default file name
                DefaultExt = ".epub",         // Default file extension
                Filter = "Epub files (*.epub)|*.epub|All files (*.*)|*.*"  // Filter files by extension
            };

            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                string filePath = saveFileDialog.FileName;
                ZipFile.CreateFromDirectory(bookModel.cachePathExtracted, filePath);
            }
        }
        public void SaveAsOnlyAnnotation()
        {
            using var session = dataBase.OpenSession();
            var anns = session.Query<Annotation>().ToList();
            if (anns.Count == 0)
            {
                MessageBox.Show("No Annotation Found!");
                return;
            }
            AnnotationSet annSet=new(bookRef, anns);
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = bookRef.Title,       // Default file name
                DefaultExt = ".ann",         // Default file extension
                Filter = "Annotation files (*.ann)|*.ann|All files (*.*)|*.*"  // Filter files by extension
            };

            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                string filePath = saveFileDialog.FileName;
                File.WriteAllText(filePath, JsonConvert.SerializeObject(annSet, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented  // optional: for readable output
                }));
            }
        }
        public void SaveAsPackIntoEPUB()
        {
            using var session = dataBase.OpenSession();
            var anns = session.Query<Annotation>().ToList();
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = bookRef.Title,       // Default file name
                DefaultExt = ".epub",         // Default file extension
                Filter = "Epub files (*.epub)|*.epub|All files (*.*)|*.*"  // Filter files by extension
            };

            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                string filePath = saveFileDialog.FileName;
                if (File.Exists(filePath))
                {
                    Console.WriteLine("Target archive exists. Deleting it.");
                    File.Delete(filePath);
                }
                ZipFile.CreateFromDirectory(bookModel.cachePathExtracted, filePath);
                if(anns.Count > 0)
                {
                    AnnotationSet annSet = new(bookRef, anns);
                    using (FileStream zipToOpen = new FileStream(filePath, FileMode.Open))
                    {
                        using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                        {
                            ZipArchiveEntry readmeEntry = archive.CreateEntry("META-INF/annotations.ann");
                            using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                            {
                                writer.Write(JsonConvert.SerializeObject(annSet, new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore,
                                    Formatting = Formatting.Indented  // optional: for readable output
                                }));
                            }
                        }
                    }
                }
               
            }

        }
    }
}
