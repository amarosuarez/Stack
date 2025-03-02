namespace Stack.Pages;

public partial class UnirseSalaPage : ContentPage
{
	public UnirseSalaPage()
	{
		InitializeComponent();
	}

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///home");
    }

    private async void OnUnirseSalaClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///partida");
    }
}