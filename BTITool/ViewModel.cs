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
            if (ImageList == null)
            {
                return;
            }
            foreach (BinaryTextureImage img in ImageList)
            {
                SaveFileDialog saveFile = new SaveFileDialog();
                saveFile.FileName = System.IO.Path.GetFileNameWithoutExtension(img.Name);
                saveFile.DefaultExt = ".bmp";
                saveFile.Filter = "Bitmap Graphics (*.bmp)|*.bmp|All Files (*.*)|*.*";
                saveFile.FilterIndex = 0;
                saveFile.RestoreDirectory = true;
                if (saveFile.ShowDialog() == true) //nullable bool, have to explicitly compare against true
                {
                    img.SaveImageToDisk(saveFile.FileName, img.GetData(), img.Width, img.Height);
                }
            }
        }

        private void ClearImageList()
        {
            ImageList.Clear();
        }
        #endregion

        #region Misc.
        private void OpenWebpage(string address)
        {
            System.Diagnostics.Process.Start(address);
        }

        private void OpenAboutWindow()
        {
            AboutWindow win = new AboutWindow();
            win.ShowDialog();
        }
        #endregion

        #region Command Callbacks
        public ICommand SaveAll
        {
            get { return new RelayCommand(x => SaveAllImages());  }
        }
        /// <summary> The user has requested to open an image/some images. </summary>
        public ICommand OnRequestOpenImages
        {
            get { return new RelayCommand(x => OpenImages()); }
        }

        /// <summary> The user has requested to clear the current list of images. </summary>
        public ICommand OnRequestClearList
        {
            get { return new RelayCommand(x => ClearImageList(), x => ImageList != null); }
        }

        /// <summary> The user has requested to be sent to the Wiki page of the Github project. </summary>
        public ICommand OnRequestOpenWiki
        {
            get { return new RelayCommand(x => OpenWebpage("https://github.com/Sage-of-Mirrors/BTITool/wiki")); }
        }

        /// <summary> The user has requested to be sent to the Issues page of the Github project. </summary>
        public ICommand OnRequestReportBug
        {
            get { return new RelayCommand(x => OpenWebpage("https://github.com/Sage-of-Mirrors/BTITool/issues")); }
        }

        /// <summary> The user has requested to be sent to the Issues page of the Github project. </summary>
        public ICommand OnRequestOpenAbout
        {
            get { return new RelayCommand(x => OpenAboutWindow()); }
        }
        #endregion
    }
}
