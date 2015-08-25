using JetBrains.UI.Wpf;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity.Options
{
  [View]
  public partial class ComplexityAnalysisOptionsView : IView<ComplexityAnalysisOptionsViewModel>
  {
    public ComplexityAnalysisOptionsView()
    {
      InitializeComponent();
    }
  }
}
