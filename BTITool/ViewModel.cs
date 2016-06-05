using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using GameFormatReader.Common;
using DmitryBrant.ImageFormats;

namespace BTITool
{
    class ViewModel : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged overhead
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ObservableCollection<BinaryTextureImage> m_imageList;
        public ObservableCollection<BinaryTextureImage> ImageList
        {
            get { return m_imageList; }
            set 
            { 
                if (m_imageList != value)
                {
                    m_imageList = value;
                    NotifyPropertyChanged();
                }
            }
        }
        
        public ViewModel()
        {

        }

        #region Input/Output
        private void OpenImages()
        {
            OpenFileDialog openDialog = new OpenFileDialog();

            openDialog.Multiselect = true;
            openDialog.Filter = "All Supported Formats (*.bti, *.png, *.tga)|*.bti;*.png;*.tga|Nintendo Images (*.bti)|*.bti|Standard Images (*.png, *.tga)|*.png;*.tga|All files (*.*)|*.*";

            if (openDialog.ShowDialog() == true)
            {
                ImageList = LoadImages(openDialog.FileNames);
            }
        }
        private ObservableCollection<BinaryTextureImage> LoadImages(string[] paths)
        {
            if (ImageList == null)
                ImageList = new ObservableCollection<BinaryTextureImage>();

            ObservableCollection<BinaryTextureImage> tempList = ImageList;

            foreach (string path in paths)
            {
                BinaryTextureImage openedImage = null;

                // Open a BTI
                if (path.EndsWith(".bti"))
                {
                    using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);

                        openedImage = new BinaryTextureImage();

                        openedImage.Load(reader, path, 0);
                    }
                }
                // Open a PNG
                else if (path.EndsWith(".png") || path.EndsWith(".PNG"))
                {
                    Bitmap input = new Bitmap(path);

                    openedImage = new BinaryTextureImage(path, input, BinaryTextureImage.TextureFormats.PNG);
                }
                // Open a TGA
                else if (path.EndsWith(".tga"))
                {
                    Bitmap input = TgaReader.Load(path);

                    openedImage = new BinaryTextureImage(path, input, BinaryTextureImage.TextureFormats.TGA);
                }

                tempList.Add(openedImage);
            }

            return tempList;
        }

        private void SaveSelectedImages()
        {

        }

        private void SaveAllImages()
        {

        }

        private void ClearImageList()
        {
            ImageList.Clear();
        }
        #endregion

        #region Command Callbacks
        /// <summary> The user has requested to open an image/some images. </summary>
        public ICommand OnRequestOpenImages
        {
            get { return new RelayCommand(x => OpenImages()); }
        }

        /// <summary> The user has requested to open an image/some images. </summary>
        public ICommand OnRequestClearList
        {
            get { return new RelayCommand(x => ClearImageList(), x => ImageList != null); }
        }
        #endregion
    }
}
