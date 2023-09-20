﻿using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using static System.Net.WebRequestMethods;
using System.Timers;

namespace myApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        Dictionary<string, BitmapImage> dict = new();
        string folderPath = "C:\\";
        bool timeOver = false;

        public MainWindow()
        {
            InitializeComponent();

            LoadFolder();
            //LoadFolderDank();
            xlistbox.ItemsSource = dict.Keys;
            xtextbox.Text = folderPath;
        }
        private void listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                BitmapImage? img;
                string? file = (xlistbox.SelectedItem.ToString());

                dict.TryGetValue(file, out img);

                ximage.Source = img;

            }
            catch (NullReferenceException)
            {
                return;
            }
        }

        private void FolderButton_Clicked(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = folderPath;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                folderPath = dialog.FileName;
                LoadFolder();

                xtextbox.Text = dialog.FileName;
                xsavepath.Text = dialog.FileName;
            }
        }

        private void LoadFolder()
        {
            dict = new Dictionary<string, BitmapImage>();
            IEnumerable<string> files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".jpeg") || s.EndsWith(".jpg") || s.EndsWith(".png"));

            foreach (string file in files)
            {
                var hello = new BitmapImage(new Uri(@$"{file}"));
                string fileName = file.Remove(0, folderPath.Length + 1);
                dict.Add(fileName, hello);

                Debug.WriteLine(file);
            }
            xlistbox.ItemsSource = dict.Keys;
        }

        private void LoadFolderDank()
        {
            string folderPath = "F:\\Unity\\Mobile Game - kopia\\Assets\\Resources";
            dict = new Dictionary<string, BitmapImage>();
            string[] directories = Directory.GetDirectories(folderPath, "*.*", SearchOption.TopDirectoryOnly);

            bool imageExists;
            bool portraitExists;

            for (int i = 0; i < directories.Length; i++)
            {
                var hello = new BitmapImage();
                var portrait = new BitmapImage();
                imageExists = true;
                portraitExists = true;


                try
                {
                    hello = new BitmapImage(new Uri(@$"{directories[i]}\fullimage.jpg"));

                }
                catch (FileNotFoundException)
                {
                    try
                    {
                        hello = new BitmapImage(new Uri(@$"{directories[i]}\fullimage.png"));

                    }
                    catch (FileNotFoundException)
                    {
                        imageExists = false;
                    }

                }
                try
                {
                    portrait = new BitmapImage(new Uri(@$"{directories[i]}\portrait.png"));

                }
                catch (FileNotFoundException)
                {
                    try
                    {
                        portrait = new BitmapImage(new Uri(@$"{directories[i]}\portrait.jpg"));

                    }
                    catch (FileNotFoundException)
                    {
                        portraitExists = false;
                    }

                }

                if (imageExists)
                {
                    directories[i] = directories[i].Remove(0, folderPath.Length + 1);
                    if (!portraitExists)
                    {
                        dict.Add(directories[i], hello);
                    }


                }

                Debug.WriteLine(directories[i]);
            }
            xlistbox.ItemsSource = dict.Keys;
        }




        bool mouseDown = false; // Set to 'true' when mouse is held down.
        bool mouseOverImg = false;
        bool noCropping = false;
        Point mouseDownPos; // The point where the mouse button was clicked down.
        Point imgDownPos;

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!mouseOverImg)
            {
                Debug.WriteLine("NO CROPPING");
                noCropping = true;
                return;

            }
            Debug.WriteLine("Cropping");
            noCropping = false;
            // Capture and track the mouse.
            mouseDown = true;
            mouseDownPos = e.GetPosition(theGrid);
            imgDownPos = e.GetPosition(ximage);
            theGrid.CaptureMouse();

            // Initial placement of the drag selection box.         
            Canvas.SetLeft(selectionBox, mouseDownPos.X);
            Canvas.SetTop(selectionBox, mouseDownPos.Y);
            selectionBox.Width = 0;
            selectionBox.Height = 0;

            // Make the drag selection box visible.
            selectionBox.Visibility = Visibility.Visible;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (noCropping)
            {
                Debug.WriteLine("ok no cropping");
                return;
            }
            Debug.WriteLine("YES LETS CROP!!");
            noCropping = true;

            // Release the mouse capture and stop tracking it.
            mouseDown = false;
            theGrid.ReleaseMouseCapture();

            // Hide the drag selection box.
            selectionBox.Visibility = Visibility.Collapsed;

            //Point mouseUpPos = e.GetPosition(theGrid);
            Point imgUpPos = e.GetPosition(ximage);

            int imgX;
            int imgY;
            int width;
            int height;
            var source = (BitmapSource)ximage.Source;
            bool square = (bool)xcheckbox.IsChecked;
            bool upsideX = false;
            bool upsideY = false;

            double scale = source.PixelWidth / ximage.ActualWidth;

            if (imgDownPos.X < imgUpPos.X)
            {
                imgX = (int)(imgDownPos.X * scale);
                width = (int)(imgUpPos.X * scale - imgX);
            }
            else
            {
                imgX = (int)(imgUpPos.X * scale);
                width = (int)(imgDownPos.X * scale - imgX);
                upsideX = true;
            }
            if (imgDownPos.Y < imgUpPos.Y)
            {
                imgY = (int)(imgDownPos.Y * scale);
                height = (int)(imgUpPos.Y * scale - imgY);
            }
            else
            {
                imgY = (int)(imgUpPos.Y * scale);
                height = (int)(imgDownPos.Y * scale - imgY);
                upsideY = true;
            }

            if (square)
            {
                if (width > height)
                {
                    width = height;
                }
                else
                {
                    height = width;
                }
                if (upsideX)
                {
                    imgX = (int)(imgDownPos.X * scale - width); 
                }
                if (upsideY)
                {
                    imgY = (int)(imgDownPos.Y* scale - height);
                }

            }


            if (imgX > 0 && imgY > 0 && width > 0 && height > 0 && imgX + width < source.PixelWidth && imgY + height < source.PixelHeight)
            {
                // Create a CroppedBitmap based off of a xaml defined resource.
                CroppedBitmap cb = new CroppedBitmap(
                    (BitmapSource)ximage.Source,
                    new Int32Rect(imgX, imgY, width, height));       //select region rect
                xcroped.Source  = cb;                 //set image source to cropped

            }

        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (timeOver)
            {
                xsaveinfo.Text = "";
                xsaveinfo.Background = new SolidColorBrush(Colors.Transparent);
                timeOver = false;
            }
            
            if (!mouseDown)
            {
                return;
            }

            Point mousePos = e.GetPosition(theGrid);
            bool square = (bool)xcheckbox.IsChecked;
            bool upsideX = false;
            bool upsideY = false;

            if (mouseDownPos.X < mousePos.X)
            {
                
                Canvas.SetLeft(selectionBox, mouseDownPos.X);
                selectionBox.Width = mousePos.X - mouseDownPos.X;
            }
            else
            {
                if (!square)
                {
                    Canvas.SetLeft(selectionBox, mousePos.X);
                }
                upsideX = true;
                selectionBox.Width = mouseDownPos.X - mousePos.X;
            }

            if (mouseDownPos.Y < mousePos.Y)
            {
                Canvas.SetTop(selectionBox, mouseDownPos.Y);
                selectionBox.Height = mousePos.Y - mouseDownPos.Y;
            }
            else
            {
                if (!square)
                {
                    Canvas.SetTop(selectionBox, mousePos.Y);
                }
                upsideY = true;
                
                selectionBox.Height = mouseDownPos.Y - mousePos.Y;
            }
            if (!square)
            {
                return;
            }

            if (selectionBox.Width < selectionBox.Height)
            {
               selectionBox.Height = selectionBox.Width;
            }
            else
            {
                selectionBox.Width = selectionBox.Height;
            }
            if (upsideX)
            {
                Canvas.SetLeft(selectionBox, mouseDownPos.X - selectionBox.Width);
            }
            if (upsideY)
            {
                Canvas.SetTop(selectionBox, mouseDownPos.Y - selectionBox.Height);
            }



        }

        private void ximage_MouseEnter(object sender, MouseEventArgs e)
        {
            xborder.BorderBrush = new SolidColorBrush(Colors.Green);
            mouseOverImg = true;
        }

        private void ximage_MouseLeave(object sender, MouseEventArgs e)
        {
            xborder.BorderBrush = new SolidColorBrush(Colors.Fuchsia);
            mouseOverImg = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                BitmapSource img = (BitmapSource)xcroped.Source;

                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img));

                using (var fileStream = new FileStream($"{xsavepath.Text}\\{xfilename.Text}.png", FileMode.Create))
                {
                    encoder.Save(fileStream);
                }

                //using (var fileStream = new FileStream($"F:\\Unity\\Mobile Game - kopia\\Assets\\Resources\\{xlistbox.SelectedValue}\\portrait.png", FileMode.Create))
                //{
                //    encoder.Save(fileStream);
                //}

                xsaveinfo.Text = "Save succesfull";
                xsaveinfo.Background = new SolidColorBrush(Colors.Green);


            }
            catch (ArgumentNullException)
            {
                xsaveinfo.Text = "ERROR! There is no image to save";
                xsaveinfo.Background = new SolidColorBrush(Colors.Red);
            }
            catch (DirectoryNotFoundException)
            {
                xsaveinfo.Text = "ERROR! Could not find Directory";
                xsaveinfo.Background = new SolidColorBrush(Colors.Red);
            }
            catch (UnauthorizedAccessException)
            {
                xsaveinfo.Text = "ERROR! Program is not authorized to save into chosen directory";
                xsaveinfo.Background = new SolidColorBrush(Colors.Red);
            }
            catch (IOException)
            {
                xsaveinfo.Text = "ERROR! Unsuported characters in filename";
                xsaveinfo.Background = new SolidColorBrush(Colors.Red);

            }
            var scheduler = new Scheduler();

            scheduler.Execute(() => timeOver = true, 4000);

        }

    }
}



//var dialog = new Microsoft.Win32.OpenFileDialog();
//dialog.InitialDirectory = "c:\\";
//dialog.Filter = ""; // Filter files by extension
//dialog.ShowDialog();

