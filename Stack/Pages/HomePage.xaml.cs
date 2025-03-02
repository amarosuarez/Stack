namespace Stack.Pages;

public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
	}

    private async void OnCrearSalaClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///crearSala");
    }

    private async void OnUnirseSalaClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///unirseSala");
    }
}