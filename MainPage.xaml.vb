Imports Windows.Storage
Imports Windows.Storage.Pickers
Imports Windows.Storage.Streams
Imports Windows.UI.Input.Inking

Public NotInheritable Class MainPage
    Inherits Page
    Public Property _removed As IReadOnlyList(Of InkStroke)
    Public Sub New()
        Me.InitializeComponent()
        Inky.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse Or Windows.UI.Core.CoreInputDeviceTypes.Pen
        Inky.InkPresenter.StrokeContainer.GetStrokes()
    End Sub
    Private Async Sub OpenButton_Click(sender As Object, e As RoutedEventArgs) Handles OpenButton.Click
        Dim openPicker As Windows.Storage.Pickers.FileOpenPicker = New Windows.Storage.Pickers.FileOpenPicker()
        openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
        openPicker.FileTypeFilter.Add(".gif")
        Dim file As Windows.Storage.StorageFile = Await openPicker.PickSingleFileAsync()
        If file IsNot Nothing Then
            Dim stream As IRandomAccessStream = Await file.OpenAsync(Windows.Storage.FileAccessMode.Read)
            Using inputStream = stream.GetInputStreamAt(0)
                Await Inky.InkPresenter.StrokeContainer.LoadAsync(inputStream)
            End Using
            stream.Dispose()
        Else
        End If
    End Sub
    Private Async Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        Dim CurrentStrokes As IReadOnlyList(Of InkStroke) = Inky.InkPresenter.StrokeContainer.GetStrokes()
        If CurrentStrokes.Count > 0 Then
            Dim SavePicker As FileSavePicker = New FileSavePicker()
            SavePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            SavePicker.FileTypeChoices.Add("GIF with embedded ISF (Ink Serialized Format)", New List(Of String)() From {".gif"})
            SavePicker.DefaultFileExtension = ".gif"
            SavePicker.SuggestedFileName = "RembrandtDrawing"
            Dim File As StorageFile = Await SavePicker.PickSaveFileAsync()
            If File IsNot Nothing Then
                Windows.Storage.CachedFileManager.DeferUpdates(File)
                Dim Stream As IRandomAccessStream = Await File.OpenAsync(FileAccessMode.ReadWrite)
                Using OutputStream As IOutputStream = Stream.GetOutputStreamAt(0)
                    Await Inky.InkPresenter.StrokeContainer.SaveAsync(OutputStream)
                    Await OutputStream.FlushAsync()
                End Using
                Stream.Dispose()
                Dim status As Windows.Storage.Provider.FileUpdateStatus = Await CachedFileManager.CompleteUpdatesAsync(File)
                If status = Windows.Storage.Provider.FileUpdateStatus.Complete Then
                Else
                End If
            Else
            End If
        End If
    End Sub
    Private Async Sub DeleteButton_Click(sender As Object, e As RoutedEventArgs) Handles DeleteButton.Click
        Dim deleteDialog As ContentDialog = New ContentDialog() With {
            .Title = "Delete Drawing?",
            .Content = "Do you want to delete this drawing?",
            .PrimaryButtonText = "Yes",
            .CloseButtonText = "No"
        }
        Dim result As ContentDialogResult = Await deleteDialog.ShowAsync()
        If result = ContentDialogResult.Primary Then
            Inky.InkPresenter.StrokeContainer.Clear()
        End If
    End Sub
End Class