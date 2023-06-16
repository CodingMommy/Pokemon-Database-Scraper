// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.UI.Popups;
using System.Diagnostics;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PokemonDatabaseScraper.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CardLibraryView : Page
    {
        public CardLibraryView()
        {
            this.InitializeComponent();
            CardGrid.Source = Globals.CardSets;
            /// GenerateCards();
        }

        private void GenerateCards()
        {
            CardSet newCardSet = new CardSet()
            { Name = "BW1" };
            newCardSet.Cards.Add(new Card()
            { Name = "Snivy", CardNumber = 1, ImageUri = new Uri(this.BaseUri, "\\Assets\\Cards\\BW1_EN_1.png") });
            Globals.CardSets.Add(newCardSet);
        }
    }
}
