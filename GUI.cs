using System;
using Autodesk.Windows;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace AutoCadGcode
{
    public class GUI
    {
        RibbonButton setPumpingTrueButton = new RibbonButton();
        RibbonButton setPumpingFalseButton = new RibbonButton();
        RibbonPanelSource rbPanelPumpingSource = new RibbonPanelSource();
        RibbonPanel rbPumpingPanel = new RibbonPanel();

        RibbonButton validateEntityes = new RibbonButton();
        RibbonPanelSource rbPanelValidateSource = new RibbonPanelSource();
        RibbonPanel rbValidatePanel = new RibbonPanel();

        RibbonTab rbTab = new RibbonTab();
        RibbonControl rbCntrl = ComponentManager.Ribbon;

        public GUI()
        {
            CreateGUI();
        }

        private void CreateGUI()
        {
            /**
             * Properties
             */
            setPumpingTrueButton.Id = "_setPumpingTrueButton";
            setPumpingTrueButton.CommandHandler = new SetPumpingTrueHandler();
            setPumpingTrueButton.Size = RibbonItemSize.Standard;
            //setPumpingTrueButton.Width = 32;
            //setPumpingTrueButton.Height = 32;
            setPumpingTrueButton.Text = "С бетоном";
            setPumpingTrueButton.ShowText = true;
            //setPumpingTrueButton.Image = new BitmapImage(new Uri("AutoCadGcode/ico/PumpingTrue_32.png", UriKind.Relative));
            //setPumpingTrueButton.ShowImage = true;


            setPumpingFalseButton.Id = "_setPumpingFalseButton";
            setPumpingFalseButton.CommandHandler = new SetPumpingFalseHandler();
            setPumpingFalseButton.Size = RibbonItemSize.Standard;
            //setPumpingFalseButton.Width = 32;
            //setPumpingFalseButton.Height = 32;
            setPumpingFalseButton.Text = "Без бетона";
            setPumpingFalseButton.ShowText = true;
            //setPumpingFalseButton.Image = new BitmapImage(new Uri("pack://application:,,,AutoCadGcode;component/ico/PumpingFalse_32.png"));
            //setPumpingFalseButton.ShowImage = true;

            rbPanelPumpingSource.Title = "Set Pumping";
            rbPanelPumpingSource.Items.Add(setPumpingTrueButton);
            rbPanelPumpingSource.Items.Add(new RibbonSeparator());
            rbPanelPumpingSource.Items.Add(setPumpingFalseButton);
            rbPumpingPanel.Source = rbPanelPumpingSource;

            /**
             * Validation and building
             */

            validateEntityes.Id = "_validateEntityesButton";
            validateEntityes.CommandHandler = new ValidateEntityesHandler();
            validateEntityes.Size = RibbonItemSize.Standard;
            //setPumpingTrueButton.Width = 32;
            //setPumpingTrueButton.Height = 32;
            validateEntityes.Text = "Валидация";
            validateEntityes.ShowText = true;

            rbPanelValidateSource.Title = "Validation";
            rbPanelValidateSource.Items.Add(validateEntityes);
            rbValidatePanel.Source = rbPanelValidateSource;

            rbTab.Title = "Trace Maker";
            rbTab.Id = "_TraceMakerTab";

            rbTab.Panels.Add(rbPumpingPanel);
            rbTab.Panels.Add(rbValidatePanel);


            rbCntrl.Tabs.Add(rbTab);

            rbTab.IsActive = true;
        }
    }

    //System.Windows.Media.Imaging.BitmapImage LoadImage(string ImageName)
    //{
    //    return new System.Windows.Media.Imaging.BitmapImage(
    //        new Uri("pack://application:,,,/ACadRibbon;component/" + ImageName + ".png"));
    //}

    public class SetPumpingTrueHandler : System.Windows.Input.ICommand
    {

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object e)
        {
            return true;
        }

        public void Execute(object e)
        {
            API.SetPumpingTrue();
        }
    }

    public class SetPumpingFalseHandler : System.Windows.Input.ICommand
    {

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object e)
        {
            return true;
        }

        public void Execute(object e)
        {
            API.SetPumpingFalse();
        }
    }

    public class ValidateEntityesHandler : System.Windows.Input.ICommand
    {

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object e)
        {
            return true;
        }

        public void Execute(object e)
        {
            API.ValidateEntityes();
        }
    }
}
