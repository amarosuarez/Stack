using Stack.ViewModels;

namespace Stack.Pages;

public partial class PrePartidaPage : ContentPage
{
    private PrePartidaVM _viewModel;

    public PrePartidaPage()
	{
		InitializeComponent(); 
        //_viewModel = new PrePartidaVM();
        //BindingContext = _viewModel;
    }

    //protected override void OnAppearing()
    //{
    //    base.OnAppearing();
    //    _viewModel.StartCountdown(); // Iniciar el countdown cada vez que la página se muestra
    //}
}