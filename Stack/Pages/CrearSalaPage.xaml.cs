namespace Stack.Pages;

public partial class CrearSalaPage : ContentPage
{
	public CrearSalaPage()
	{
		InitializeComponent();
	}

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///home");
    }

    private async void OnCrearSalaClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///partida");
    }
}