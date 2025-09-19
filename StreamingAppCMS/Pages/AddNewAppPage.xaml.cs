using Microsoft.Win32;
using Notification.Wpf;
using StreamingAppCMS.Helpers;
using StreamingAppCMS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace StreamingAppCMS.Pages
{
    /// <summary>
    /// Interaction logic for AddNewAppPage.xaml
    /// </summary>
    public partial class AddNewAppPage : Page
    {
        MainWindow mainWindow;
        private string selectedImagePath;
        private bool isPlaceholderActive = true;
        private const string PlaceholderText = "Enter app description here...";
        private ObservableCollection<StreamingApp> streamingApps;
        StreamingApp editingApp;

        private SolidColorBrush currentTextColor = new SolidColorBrush(Colors.Black);
        private List<ColorItems> systemColors;
        private TextRange savedTextRange;
        public AddNewAppPage(ObservableCollection<StreamingApp> apps, StreamingApp appToEdit)
        {
            if (appToEdit != null)
                DataContext = appToEdit;

            mainWindow = (MainWindow)Application.Current.MainWindow;
            selectedImagePath = string.Empty;
            streamingApps = apps;
            editingApp = appToEdit;

            InitializeComponent();
            InitializeFontControls();
            SetupRichTextBoxEvents();
            InitializeColorPicker();

            if (editingApp != null)
            {
                PopulateFieldsFromApp(editingApp);
            }
        }

        private void PopulateFieldsFromApp(StreamingApp app)
        {
            if (txtAppName != null)
                txtAppName.Text = app.Name;

            if (txtNumOfUsers != null)
                txtNumOfUsers.Text = app.NumOfUsers.ToString();

            string absImagePath = GetAbsolutePath(app.ImagePath);
            if (!string.IsNullOrEmpty(absImagePath) && File.Exists(absImagePath))
            {
                selectedImagePath = app.ImagePath;
                if (imgPreview != null) 
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(Path.GetFullPath(app.ImagePath)));
                    imgPreview.Source = bitmap;
                    txtNoImage.Text = "";
                }
            }

            LoadRtfContent(app.DescriptionPath);

            if (Title != null)
                Title = $"Edit {app.Name}";

            if (btnSave != null)
                btnSave.Content = "Update App";
        }


        private void LoadRtfContent(string rtfPath)
        {
            try
            {
                string projectRoot = AppDomain.CurrentDomain.BaseDirectory;
                string fullPath = Path.Combine(projectRoot, rtfPath);
                if (File.Exists(fullPath))
                {
                    string rtfContent = File.ReadAllText(fullPath);
                    TextRange textRange = new TextRange(richTextBoxDescription.Document.ContentStart, richTextBoxDescription.Document.ContentEnd);
                    using (MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(rtfContent)))
                    {
                        textRange.Load(ms, DataFormats.Rtf);
                        isPlaceholderActive = false;
                    }
                }
                else
                {       
                    string binDebugPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rtfPath);
                    if (File.Exists(binDebugPath))
                    {
                        string rtfContent = File.ReadAllText(binDebugPath);
                        TextRange textRange = new TextRange(richTextBoxDescription.Document.ContentStart, richTextBoxDescription.Document.ContentEnd);
                        using (MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(rtfContent)))
                        {
                            textRange.Load(ms, DataFormats.Rtf);
                            isPlaceholderActive = false;
                        }
                    }
                    else
                    {
                        string message = $"File not found in either location:\n{fullPath}\n{binDebugPath}";
                        mainWindow.ShowToastNotification(new ToastNotification("Erorr", message, NotificationType.Error));
                    }
                }
            }
            catch (Exception ex)
            {
                string message = $"Error loading RTF file: {ex.Message}";
                mainWindow.ShowToastNotification(new ToastNotification("Erorr", message, NotificationType.Error));
            }
        }

        private void UpdateExistingApp()
        {
            string descriptionPath = SaveDescriptionToFile(txtAppName.Text.Trim());

            editingApp.Name = txtAppName.Text.Trim();
            editingApp.NumOfUsers = int.Parse(txtNumOfUsers.Text);
            editingApp.ImagePath = selectedImagePath;
            editingApp.DescriptionPath = descriptionPath;

            StreamingAppDataStorage.SaveApps(streamingApps.ToList());

            mainWindow.ShowToastNotification(new ToastNotification("Success", "Application successfully updated!", NotificationType.Success));
        }
        private void CreateNewApp()
        {
            StreamingApp newApp = new StreamingApp
            {
                Name = txtAppName.Text,
                NumOfUsers = int.Parse(txtNumOfUsers.Text),
                ImagePath = selectedImagePath,
                DateAdded = DateTime.Now
            };

            string descriptionPath = SaveDescriptionToFile(newApp.Name);
            newApp.DescriptionPath = descriptionPath;

            streamingApps.Add(newApp);

            StreamingAppDataStorage.AddApp(newApp);

            mainWindow.ShowToastNotification(new ToastNotification("Success", "Application successfully added!", NotificationType.Success));
        }

        private string SaveDescriptionToFile(string appName)
        {
            string fileName = $"{appName.Replace(" ", "")}Description.rtf";
            string filePath = Path.Combine("Assets", "Descriptions", fileName);

            string fullDirectoryPath = Path.GetDirectoryName(Path.GetFullPath(filePath));
            Directory.CreateDirectory(fullDirectoryPath);

            if (richTextBoxDescription != null)
            {
                TextRange textRange = new TextRange(richTextBoxDescription.Document.ContentStart,
                                                  richTextBoxDescription.Document.ContentEnd);
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    textRange.Save(fileStream, DataFormats.Rtf);
                }
            }
            return filePath;
        }
        private void SetupRichTextBoxEvents()
        {
            richTextBoxDescription.GotFocus += RichTextBoxDescription_GotFocus;
            richTextBoxDescription.LostFocus += RichTextBoxDescription_LostFocus;
        }

        private void RichTextBoxDescription_GotFocus(object sender, RoutedEventArgs e)
        {
            if (isPlaceholderActive)
            {
                richTextBoxDescription.Document.Blocks.Clear();
                isPlaceholderActive = false;
            }

            if (currentTextColor != null)
            {
                richTextBoxDescription.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, currentTextColor);
            }
        }

        private void RichTextBoxDescription_LostFocus(object sender, RoutedEventArgs e)
        {
            string content = GetPlainTextFromRichTextBox();
            if (string.IsNullOrWhiteSpace(content))
            {
                SetPlaceholderText();
            }
        }

        private void SetPlaceholderText()
        {
            richTextBoxDescription.Document.Blocks.Clear();
            Paragraph paragraph = new Paragraph();
            Run run = new Run(PlaceholderText)
            {
                Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204)) // #CCCCCC
            };
            paragraph.Inlines.Add(run);
            richTextBoxDescription.Document.Blocks.Add(paragraph);
            isPlaceholderActive = true;
        }

        private string GetPlainTextFromRichTextBox()
        {
            TextRange textRange = new TextRange(richTextBoxDescription.Document.ContentStart, richTextBoxDescription.Document.ContentEnd);
            return textRange.Text.Trim();
        }

        private void UpdateWordCount()
        {
            if (txtWordCount == null)
                return;

            if (isPlaceholderActive)
            {
                txtWordCount.Text = "Words: 0";
                return;
            }

            string content = GetPlainTextFromRichTextBox();
            if (string.IsNullOrWhiteSpace(content))
            {
                txtWordCount.Text = "Words: 0";
                return;
            }

            string[] words = content.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            txtWordCount.Text = $"Words: {words.Length}";
        }

        private bool ValidateDescription()
        {
            txtDescriptionError.Text = string.Empty;
            txtDescriptionError.Visibility = Visibility.Collapsed;

            if (isPlaceholderActive)
            {
                txtDescriptionError.Text = "Application description is required.";
                txtDescriptionError.Visibility = Visibility.Visible;
                return false;
            }

            string content = GetPlainTextFromRichTextBox();
            if (string.IsNullOrWhiteSpace(content))
            {
                txtDescriptionError.Text = "Application description is required.";
                txtDescriptionError.Visibility = Visibility.Visible;
                return false;
            }

            if (content.Length < 10)
            {
                txtDescriptionError.Text = "Description must be at least 10 characters long.";
                txtDescriptionError.Visibility = Visibility.Visible;
                return false;
            }

            if (content.Length > 5000)
            {
                txtDescriptionError.Text = "Description cannot exceed 5000 characters.";
                txtDescriptionError.Visibility = Visibility.Visible;
                return false;
            }

            return true;
        }

        private void InitializeColorPicker()
        {
            LoadSystemColors();
            SetInitialColor();
        }

        private void SetInitialColor()
        {
            currentTextColor = new SolidColorBrush(Colors.White);
            if (colorIndicator != null)
                colorIndicator.Fill = currentTextColor;
        }
        

        private void LoadSystemColors()
        {
            systemColors = new List<ColorItems>();

            var colorProperties = typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public)
                .Where(p => p.PropertyType == typeof(Color))
                .OrderBy(p => p.Name);

            foreach (var property in colorProperties)
            {
                var color = (Color)property.GetValue(null);
                systemColors.Add(new ColorItems
                {
                    ColorName = property.Name,
                    Brush = new SolidColorBrush(color)
                });
            }

            colorListBox.ItemsSource = systemColors;
        }

        private void btnTextColor_Click(object sender, RoutedEventArgs e)
        {
            if (richTextBoxDescription != null)
            {
                var selection = richTextBoxDescription.Selection;
                if (!selection.IsEmpty)
                {
                    savedTextRange = new TextRange(selection.Start, selection.End);
                }
                else
                {
                    savedTextRange = null;
                }
            }

            var currentColorItem = systemColors.FirstOrDefault(c =>
                c.Color.A == currentTextColor.Color.A &&
                c.Color.R == currentTextColor.Color.R &&
                c.Color.G == currentTextColor.Color.G &&
                c.Color.B == currentTextColor.Color.B);

            if (currentColorItem != null)
            {
                colorListBox.SelectedItem = currentColorItem;
                colorListBox.ScrollIntoView(currentColorItem);
            }

            colorPopup.IsOpen = true;

            colorListBox.Focus();
        }

        private void colorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (colorListBox.SelectedItem is ColorItems selectedColor)
            {
                currentTextColor = selectedColor.Brush;

                colorIndicator.Fill = currentTextColor;

                ApplyTextColor();

                colorPopup.IsOpen = false;
            }
        }

        private void ApplyTextColor()
        {
            if (currentTextColor == null || richTextBoxDescription == null)
                return;

            if (savedTextRange != null)
            {
                savedTextRange.ApplyPropertyValue(TextElement.ForegroundProperty, currentTextColor);
            }
            else
            {
                var caretPosition = richTextBoxDescription.CaretPosition;
                var range = new TextRange(caretPosition, caretPosition);
                range.ApplyPropertyValue(TextElement.ForegroundProperty, currentTextColor);
            }


            richTextBoxDescription.Focus();
        }

        private void btnBold_Click(object sender, RoutedEventArgs e) 
        {
            ApplyBoldFormatting();
            UpdateBoldButtonState();
        }

        private void ApplyBoldFormatting()
        {
            if (richTextBoxDescription?.Selection == null)
                return;

            if (!richTextBoxDescription.Selection.IsEmpty)
            {
                var currentWeight = richTextBoxDescription.Selection.GetPropertyValue(TextElement.FontWeightProperty);
                if (currentWeight != DependencyProperty.UnsetValue && currentWeight.Equals(FontWeights.Bold))
                {
                    richTextBoxDescription.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
                }
                else
                {
                    richTextBoxDescription.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                }
            }
            else
            {
                var currentWeight = richTextBoxDescription.Selection.GetPropertyValue(TextElement.FontWeightProperty);
                if (currentWeight != DependencyProperty.UnsetValue && currentWeight.Equals(FontWeights.Bold))
                {
                    richTextBoxDescription.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
                }
                else
                {
                    richTextBoxDescription.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                }
            }

            richTextBoxDescription.Focus();
        }

        private void UpdateBoldButtonState()
        {
            if (richTextBoxDescription?.Selection == null || btnBold == null)
                return;

            var fontWeight = richTextBoxDescription.Selection.GetPropertyValue(TextElement.FontWeightProperty);
            if (fontWeight != DependencyProperty.UnsetValue && fontWeight.Equals(FontWeights.Bold))
            {
                btnBold.IsChecked = true;
            }
            else
            {
                btnBold.IsChecked = false;
            }
        }

        private void btnItalic_Click(object sender, RoutedEventArgs e) 
        {
            ApplyItalicFormatting();
            UpdateItalicButtonState();
        }

        private void ApplyItalicFormatting()
        {
            if (richTextBoxDescription?.Selection == null)
                return;

            var currentStyle = richTextBoxDescription.Selection.GetPropertyValue(TextElement.FontStyleProperty);

            if (currentStyle != DependencyProperty.UnsetValue && currentStyle.Equals(FontStyles.Italic))
            {
                richTextBoxDescription.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Normal);
            }
            else
            {
                richTextBoxDescription.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
            }

            richTextBoxDescription.Focus();
        }

        private void UpdateItalicButtonState()
        {
            if (richTextBoxDescription?.Selection == null || btnItalic == null)
                return;

            var fontStyle = richTextBoxDescription.Selection.GetPropertyValue(TextElement.FontStyleProperty);
            if (fontStyle != DependencyProperty.UnsetValue && fontStyle.Equals(FontStyles.Italic))
            {
                btnItalic.IsChecked = true;
            }
            else
            {
                btnItalic.IsChecked = false;
            }
        }

        private void btnUnderline_Click(object sender, RoutedEventArgs e) 
        {
            ApplyUnderlineFormatting();
            UpdateUnderlineButtonState();
        }

        private void ApplyUnderlineFormatting()
        {
            if (richTextBoxDescription?.Selection == null)
                return;

            var currentDecoration = richTextBoxDescription.Selection.GetPropertyValue(Inline.TextDecorationsProperty);

            if (currentDecoration != DependencyProperty.UnsetValue &&
                currentDecoration != null &&
                ((TextDecorationCollection)currentDecoration).Contains(TextDecorations.Underline[0]))
            {
                richTextBoxDescription.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
            }
            else
            {
                richTextBoxDescription.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
            }

            richTextBoxDescription.Focus();
        }

        private void UpdateUnderlineButtonState()
        {
            if (richTextBoxDescription?.Selection == null || btnUnderline == null)
                return;

            var textDecorations = richTextBoxDescription.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            if (textDecorations != DependencyProperty.UnsetValue &&
                textDecorations != null &&
                ((TextDecorationCollection)textDecorations).Contains(TextDecorations.Underline[0]))
            {
                btnUnderline.IsChecked = true;
            }
            else
            {
                btnUnderline.IsChecked = false;
            }
        }

        private void richTextBoxDescription_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (currentTextColor != null)
            {
                richTextBoxDescription.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, currentTextColor);
            }
        }

        private void richTextBoxDescription_SelectionChanged(object sender, RoutedEventArgs e) 
        {
            UpdateBoldButtonState();
            UpdateItalicButtonState();
            UpdateUnderlineButtonState();
            UpdateFontFamilyComboBox();
            UpdateFontSizeComboBox();
        }

        private void richTextBoxDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateWordCount();
        }

        private void cmbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        {
            if (richTextBoxDescription?.Selection == null || cmbFontFamily.SelectedItem == null)
                return;

            FontFamily selectedFont = (FontFamily)cmbFontFamily.SelectedItem;
            richTextBoxDescription.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, selectedFont);

            richTextBoxDescription.Focus();
        }
        private void cmbFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        {
            if (richTextBoxDescription?.Selection == null || cmbFontSize.SelectedItem == null)
                return;

            double selectedSize = (double)cmbFontSize.SelectedItem;
            richTextBoxDescription.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, selectedSize);

            richTextBoxDescription.Focus();
        }

        private void UpdateFontFamilyComboBox()
        {
            if (richTextBoxDescription?.Selection == null || cmbFontFamily == null)
                return;

            var fontFamily = richTextBoxDescription.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
            if (fontFamily != DependencyProperty.UnsetValue && fontFamily is FontFamily family)
            {
                cmbFontFamily.SelectedItem = family;
            }
        }

        private void UpdateFontSizeComboBox()
        {
            if (richTextBoxDescription?.Selection == null || cmbFontSize == null)
                return;

            var fontSize = richTextBoxDescription.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            if (fontSize != DependencyProperty.UnsetValue && fontSize is double size)
            {
                cmbFontSize.SelectedItem = size;
            }
        }
     
        private void btnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
            string imagesFolder = System.IO.Path.Combine(projectRoot, "Assets", "Images");

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select Application Logo",
                Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                InitialDirectory = imagesFolder
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    selectedImagePath = openFileDialog.FileName;

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedImagePath);
                    bitmap.DecodePixelWidth = 140;
                    bitmap.EndInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;

                    imgPreview.Source = bitmap;
                    imgPreview.Visibility = Visibility.Visible;
                    txtNoImage.Visibility = Visibility.Hidden;
                }
                catch (Exception)
                {
                    mainWindow.ShowToastNotification(new ToastNotification("Error", "The selected file is not a valid image!", NotificationType.Error));
                    selectedImagePath = string.Empty;
                }
            }
        }

        private bool ValidateImage(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedImagePath))
            {
                mainWindow.ShowToastNotification(new ToastNotification("Validation Error", "Please select an application logo.", NotificationType.Warning));
                return false;
            }
            return true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
                mainWindow.ShowToastNotification(new ToastNotification("Information", "Action canceled.", NotificationType.Notification));
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateAllData(sender, e))
            {
                if (editingApp != null)
                {
                    UpdateExistingApp();
                }
                else
                {
                    CreateNewApp();
                }

                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();                 
                }
            }
            else
            {
                scrollViewer.ScrollToTop();
                if (!string.IsNullOrEmpty(txtAppNameError.Text))
                {
                    txtAppName.Focus();
                }
                else if (!string.IsNullOrEmpty(txtNumOfUsersError.Text))
                {
                    txtNumOfUsers.Focus();
                }
                else if (string.IsNullOrEmpty(selectedImagePath))
                {
                    btnSelectImage.Focus();
                }
                else if (txtDescriptionError.Visibility == Visibility.Visible)
                {
                    scrollViewer.ScrollToBottom();
                    richTextBoxDescription.Focus();
                }
            }
        }

        private void btnPageBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private bool ValidateApplicationName(object sender, RoutedEventArgs e)
        {
            txtAppNameError.Text = string.Empty;
            txtAppName.BorderBrush = Brushes.Gray;

            string appName = txtAppName.Text.Trim();

            if (string.IsNullOrEmpty(appName))
            {
                txtAppNameError.Text = "Application name is required.";
                txtAppName.BorderBrush = Brushes.Red;
                return false;
            }

            if (appName.Length < 2)
            {
                txtAppNameError.Text = "Application name must be at least 2 characters long.";
                txtAppName.BorderBrush = Brushes.Red;
                return false;
            }

            if (appName.Length > 50)
            {
                txtAppNameError.Text = "Application name cannot exceed 50 characters.";
                txtAppName.BorderBrush = Brushes.Red;
                return false;
            }

            return true;
        }

        private bool ValidateNumberOfUsers(object sender, RoutedEventArgs e)
        {
            txtNumOfUsersError.Text = string.Empty;
            txtNumOfUsers.BorderBrush = Brushes.Gray;

            string numOfUsersText = txtNumOfUsers.Text.Trim();

            if (string.IsNullOrEmpty(numOfUsersText))
            {
                txtNumOfUsersError.Text = "Number of users is required.";
                txtNumOfUsers.BorderBrush = Brushes.Red;
                return false;
            }

            if (!int.TryParse(numOfUsersText, out int numberOfUsers))
            {
                txtNumOfUsersError.Text = "Please enter a valid number.";
                txtNumOfUsers.BorderBrush = Brushes.Red;
                return false;
            }

            if (numberOfUsers <= 0)
            {
                txtNumOfUsersError.Text = "Number of users must be greater than 0.";
                txtNumOfUsers.BorderBrush = Brushes.Red;
                return false;
            }

            if (numberOfUsers > 1000000000) 
            {
                txtNumOfUsersError.Text = "Number of users is too large. Server cannot handle it.";
                txtNumOfUsers.BorderBrush = Brushes.Red;
                return false;
            }

            return true;
        }

        private bool ValidateAllData(object sender, RoutedEventArgs e)
        {
            bool isAppNameValid = ValidateApplicationName(sender, e);
            bool isNumOfUsersValid = ValidateNumberOfUsers(sender, e);
            bool isImageValid = ValidateImage(sender, e);
            bool isDescriptionValid = ValidateDescription();

            return isAppNameValid && isNumOfUsersValid && isImageValid && isDescriptionValid;
        }

        private void InitializeFontControls()
        {
            var fontFamilies = Fonts.SystemFontFamilies.OrderBy(f => f.Source).ToList();
            cmbFontFamily.ItemsSource = fontFamilies;
            cmbFontFamily.SelectedItem = new FontFamily("Segoe UI");

            var fontSizes = new List<double>
            {
                8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 32, 36, 48, 72
            };

            cmbFontSize.ItemsSource = fontSizes;
            cmbFontSize.SelectedItem = 12.0;
        }

        string GetAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (Path.IsPathRooted(path))
            {
                if (File.Exists(path))
                    return path;
                else
                    return null;
            }
            else
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string combined = Path.Combine(baseDir, path);
                string fullPath = Path.GetFullPath(combined);
                if (File.Exists(fullPath))
                    return fullPath;
                else
                    return null;
            }
        }
    }
}