// Decompiled with JetBrains decompiler
// Type: Assets.Scripts.Framework.GalaSports.Core.Controller
// Assembly: GameFramework, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: SlBHLVBORy1NUEERspORZH79KnLBKjA=
// Assembly location:\Assets\Plugins\GameFramework\GameFramework.dll

using Assets.Scripts.Framework.GalaSports.Core;
using Assets.Scripts.Framework.GalaSports.Interfaces;

public class HomePanel:Panel {
	private HomeController _homeController;

	public override void Init(IModule para0) {
		base.Init(para0);
		HomeView viewScript = (HomeView)InstantiateView<HomeView>("Home/HomeView");
		RegisterView(viewScript);
		_homeController = new HomeController();
		_homeController.homeView = viewScript;
		RegisterController(_homeController);
		_homeController.Start();
	}
	public override void Show(float para0) {
		_homeController.homeView.Show();
		base.Show(para0);
		
	}
	public override void Hide() {
		_homeController.homeView.Hide();
		base.Hide();
		
	}

}