using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System.Windows.Media;

namespace Client.UI
{
    public static class ThemeHelper
    {

        public static void SetTheme(BaseTheme baseTheme, Color primaryColor, Color secondaryColor)
        {
            var paletteHelper = new PaletteHelper();

            var theme = Theme.Create(baseTheme, primaryColor, secondaryColor);

            paletteHelper.SetTheme(theme);
        }

        public static IEnumerable<Swatch> GetAvaliableColors()
        {
            return new SwatchesProvider().Swatches;
        }
}

}
