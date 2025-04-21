// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using PFXToolKitUI.Utils.RDA;

namespace PFXToolKitUI.Configurations;

public delegate void ConfigurationContextActivePageChangedEventHandler(ConfigurationContext context, ConfigurationPage? oldPage, ConfigurationPage? newPage);

public delegate void ConfigurationContextEventHandler(ConfigurationContext context);

/// <summary>
/// Stores information about the user's current changes to many different pages.
/// It also tracks which page the user is currently viewing.
/// <para>
/// An instance of this class is created whenever the settings dialog is opened
/// </para>
/// </summary>
public class ConfigurationContext {
    private HashSet<ConfigurationPage>? modifiedPages;

    private ConfigurationPage? activePage; // the page we are currently viewing

    public ConfigurationPage? ActivePage => this.activePage;

    /// <summary>
    /// Gets the pages that are currently marked as modified. This might be updated
    /// periodically and/or immediately when a page self-marks itself as modified.
    /// This collection should not be relied on entirely to check the modified state is the main point
    /// </summary>
    public IEnumerable<ConfigurationPage> ModifiedPages => this.modifiedPages ?? Enumerable.Empty<ConfigurationPage>();

    public event ConfigurationContextActivePageChangedEventHandler? ActivePageChanged;
    public event ConfigurationContextEventHandler? ModifiedPagesUpdated;

    private int modificationLevelForModifiedPages, lastModificationLevelForNotification;
    private readonly RateLimitedDispatchAction updateIsModifiedAction;

    public ConfigurationContext() {
        this.updateIsModifiedAction = RateLimitedDispatchActionBase.ForDispatcherSync(() => {
            if (this.lastModificationLevelForNotification != this.modificationLevelForModifiedPages) {
                this.lastModificationLevelForNotification = this.modificationLevelForModifiedPages;
                this.ModifiedPagesUpdated?.Invoke(this);
            }
        }, TimeSpan.FromMilliseconds(250), DispatchPriority.Background);
    }

    /// <summary>
    /// Sets the page that is currently being viewed
    /// </summary>
    /// <param name="newPage"></param>
    public void SetViewPage(ConfigurationPage? newPage) {
        ConfigurationPage? oldPage = this.activePage;
        if (oldPage == newPage) {
            return;
        }

        if (oldPage != null) {
            this.activePage = null;
            oldPage.IsModifiedChanged -= this.OnPageIsModifiedChanged;
            this.OnIsModifiedChanged(oldPage, false);
            
            ConfigurationPage.InternalSetContext(oldPage, null);
        }

        if (newPage != null) {
            this.activePage = newPage;
            newPage.IsModifiedChanged += this.OnPageIsModifiedChanged;
            if (newPage.IsModified)
                this.OnIsModifiedChanged(newPage, true);
            
            ConfigurationPage.InternalSetContext(newPage, this);
        }

        this.ActivePageChanged?.Invoke(this, oldPage, newPage);
    }

    private void OnPageIsModifiedChanged(ConfigurationPage sender) {
        this.OnIsModifiedChanged(sender, sender.IsModified);
    }

    private void OnIsModifiedChanged(ConfigurationPage page, bool isModified) {
        if (isModified) {
            if (this.modifiedPages == null || !this.modifiedPages.Contains(page)) {
                (this.modifiedPages ??= new HashSet<ConfigurationPage>()).Add(page);
                this.modificationLevelForModifiedPages++;
                this.updateIsModifiedAction.InvokeAsync();
            }
        }
        else if (this.modifiedPages != null && this.modifiedPages.Remove(page)) {
            this.modificationLevelForModifiedPages++;
            this.updateIsModifiedAction.InvokeAsync();
        }
    }

    public void OnCreated() {
    }

    public void OnDestroyed() {
    }
}