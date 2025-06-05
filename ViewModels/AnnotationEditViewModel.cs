using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NotEdgeForEpubWpf.Models.AnnotationModel;
using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NotEdgeForEpubWpf.ViewModels
{
    public partial class AnnotationEditViewModel:ObservableObject
    {
        [ObservableProperty]
        private bool isOpen=false;

        [ObservableProperty]
        private Visibility popupVisibility=Visibility.Collapsed;
        [ObservableProperty]
        private string annotationText="";

        public readonly IDocumentStore dataBase;
        public string Id {  get; set; }

        //public bool IsBookmark { get; set; } = false;

        public Action<string>? delAnnotationCallback;
        public void ShowAnnotationEdit(string id)
        {
            Id = id;//IsBookmark = false;
            using var session = dataBase.OpenSession();
            var anno = session.Load<Annotation>(id);
            if (anno == null) return;
            AnnotationText = anno.Body?.Value ?? "";
            PopupVisibility = Visibility.Visible;
            IsOpen = true;
        }


        [RelayCommand]
        private void AnnotationTextSubmit()
        {
            this.PopupVisibility = Visibility.Collapsed;
            this.IsOpen = false;
            using var session = dataBase.OpenSession();
            //if (IsBookmark)
            //{
            //    var anno = session.Load<AnnotationBookmark>(Id);
            //    if (anno == null) return;
            //    if (anno.Body != null)
            //    {
            //        anno.Body.Value = AnnotationText;
            //    }
            //    else
            //    {
            //        anno.Body = new(AnnotationText);
            //    }
            //}
            //else
            {
                var anno = session.Load<Annotation>(Id);
                if (anno == null) return;
                if (anno.Body != null)
                {
                    anno.Body.Value = AnnotationText;
                }
                else
                {
                    anno.Body = new(AnnotationText);
                }
            }
            
            session.SaveChanges();
        }

        [RelayCommand]
        private void AnnotationDelete()
        {
            MessageBoxResult result = MessageBox.Show("是否确定删除此条批注？ \nwill delete this annotation, sure?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            this.PopupVisibility=Visibility.Collapsed;
            this.IsOpen = false;
            delAnnotationCallback?.Invoke(Id);
            using var session = dataBase.OpenSession();
            //if (IsBookmark)
            //{
            //    var anno = session.Load<AnnotationBookmark>(Id);
            //    if (anno == null) return;
            //    session.Delete(anno);
            //}
            //else
            {
                var anno = session.Load<Annotation>(Id);
                if (anno == null) return;
                session.Delete(anno);
            }
                
            session.SaveChanges();
        }

        public AnnotationEditViewModel(IDocumentStore dataBase)
        {
            this.dataBase = dataBase;
        }
    }
}
