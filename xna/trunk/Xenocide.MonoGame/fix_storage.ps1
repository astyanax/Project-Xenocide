# Phase 1 - manual fixes script
$src = Join-Path $PSScriptRoot "Source"

# ============================================================
# 1. FileUtil.cs - Storage -> direct file I/O
# ============================================================
$f = "$src\Utils\FileUtil.cs"
$c = [System.IO.File]::ReadAllText($f)
$old = @'
    static class FileUtil
    {
        /// <summary>
        /// Get the container (directory) holding the requested file
        /// </summary>
        /// <param name="pathName">Full path to requested file</param>
        /// <returns>the container</returns>
        public static StorageContainer GetContainer(string pathName)
        {
            // this bit is dummy on windows
            string directory = Path.GetDirectoryName(pathName);
            IAsyncResult result = Guide.BeginShowStorageDeviceSelector(PlayerIndex.One, null, null);
            StorageDevice device = Guide.EndShowStorageDeviceSelector(result);
            // Now open container(directory)
            return device.OpenContainer(directory);
        }
        /// <summary>
        /// Get correct pathname to use to get a file
        /// Allow for path being different on an X-Box
        /// </summary>
        /// <param name="container">container holding the file</param>
        /// <param name="pathName">full path</param>
        /// <returns>correct path</returns>
        public static string TruePathName(StorageContainer container, string pathName)
        {
            return Path.Combine(container.Path, Path.GetFileName(pathName));
        }
        /// <summary>
        /// Will check to see if a file exists.
        /// </summary>
        /// <param name="pathName">File Name to check for</param>
        /// <returns></returns>
        public static bool DoesFileExist(string pathName)
        {
            using (StorageContainer container = GetContainer(pathName))
            {
                return File.Exists(TruePathName(container, pathName));
            }
        }
    }
'@
$new = @'
    static class FileUtil
    {
        /// <summary>
        /// Get the save data directory path
        /// </summary>
        public static string SaveDirectory
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "XeNAcide"); }
        }
        /// <summary>
        /// Will check to see if a file exists.
        /// </summary>
        /// <param name="pathName">File Name to check for</param>
        /// <returns></returns>
        public static bool DoesFileExist(string pathName)
        {
            return File.Exists(pathName);
        }
    }
'@
$c = $c.Replace($old, $new)
[System.IO.File]::WriteAllText($f, $c, [System.Text.Encoding]::UTF8)
Write-Host "1. Fixed FileUtil.cs"

# ============================================================
# 2. GameOptions.cs - StorageContainer fix
# ============================================================
$f = "$src\Model\GameOptions.cs"
$c = [System.IO.File]::ReadAllText($f)
$c = $c.Replace(
    'using (StorageContainer container = FileUtil.GetContainer(gameOptionsPathName))
                using (var f = new FileStream(FileUtil.TruePathName(container, gameOptionsPathName), FileMode.Open))',
    'using (var f = new FileStream(gameOptionsPathName, FileMode.Open))'
)
$c = $c.Replace(
    'using (StorageContainer container = FileUtil.GetContainer(gameOptionsPathName))
                using (var f = new FileStream(FileUtil.TruePathName(container, gameOptionsPathName), FileMode.CreateNew))',
    'using (var f = new FileStream(gameOptionsPathName, FileMode.CreateNew))'
)
[System.IO.File]::WriteAllText($f, $c, [System.Text.Encoding]::UTF8)
Write-Host "2. Fixed GameOptions.cs"

# ============================================================
# 3. StaticTables.cs - TitleLocation fix
# ============================================================
$f = "$src\Model\StaticTables.cs"
$c = [System.IO.File]::ReadAllText($f)
$c = $c.Replace('StorageContainer.TitleLocation', 'AppDomain.CurrentDomain.BaseDirectory')
[System.IO.File]::WriteAllText($f, $c, [System.Text.Encoding]::UTF8)
Write-Host "3. Fixed StaticTables.cs"

# ============================================================
# 4. CreditsScreen.cs - TitleLocation fix
# ============================================================
$f = "$src\UI\Screens\CreditsScreen.cs"
$c = [System.IO.File]::ReadAllText($f)
$c = $c.Replace('StorageContainer.TitleLocation', 'AppDomain.CurrentDomain.BaseDirectory')
[System.IO.File]::WriteAllText($f, $c, [System.Text.Encoding]::UTF8)
Write-Host "4. Fixed CreditsScreen.cs"

# ============================================================
# 5. LoadSaveGameScreen.cs - StorageContainer fix
# ============================================================
$f = "$src\UI\Screens\LoadSaveGameScreen.cs"
$c = [System.IO.File]::ReadAllText($f)

$oldBlock = @'
        private void AddSaveGamesToGrid()
        {
            using (StorageContainer container = GetContainer())
            {
                ICollection<string> FileList = Directory.GetFiles(container.Path);
                foreach (string filename in FileList)
                {
                    using (FileStream stream = File.Open(filename, FileMode.Open))
                    {
                        AddSaveGameToGrid(stream, Path.GetFileName(filename));
                    }
                }
            }
        }
'@
$newBlock = @'
        private void AddSaveGamesToGrid()
        {
            string saveDir = GetSaveDirectory();
            if (Directory.Exists(saveDir))
            {
                ICollection<string> FileList = Directory.GetFiles(saveDir);
                foreach (string filename in FileList)
                {
                    using (FileStream stream = File.Open(filename, FileMode.Open))
                    {
                        AddSaveGameToGrid(stream, Path.GetFileName(filename));
                    }
                }
            }
        }
'@
$c = $c.Replace($oldBlock, $newBlock)

# Fix AddSaveGameToGrid(string)
$oldBlock = @'
        private void AddSaveGameToGrid(string filename)
        {
            using (StorageContainer container = GetContainer())
            {
                using (FileStream stream = File.Open(Path.Combine(container.Path, filename), FileMode.Open))
                {
                    AddSaveGameToGrid(stream, filename);
                }
            }
        }
'@
$newBlock = @'
        private void AddSaveGameToGrid(string filename)
        {
            string saveDir = GetSaveDirectory();
            using (FileStream stream = File.Open(Path.Combine(saveDir, filename), FileMode.Open))
            {
                AddSaveGameToGrid(stream, filename);
            }
        }
'@
$c = $c.Replace($oldBlock, $newBlock)

# Fix ReadFromFile
$oldBlock = @'
            using (StorageContainer container = GetContainer())
            {
                using (FileStream stream = File.Open(Path.Combine(container.Path, filename), FileMode.Open))
                {
                    // check version from header
                    SaveGameHeader saveGameHeader = new SaveGameHeader(stream);
                    if (saveGameHeader.IsSameVersion(Xenocide.CurrentVersion))
                    {
                        // get the game state
                        BinaryFormatter formatter = new BinaryFormatter();
                        return (GameState)formatter.Deserialize(stream);
                    }
                    else
                    {
                        Util.ShowMessageBox(Strings.SCREEN_LOADSAVEGAME_VERSION_CONFLICT);
                        return null;
                    }
                }
            }
'@
$newBlock = @'
            string saveDir = GetSaveDirectory();
            using (FileStream stream = File.Open(Path.Combine(saveDir, filename), FileMode.Open))
            {
                // check version from header
                SaveGameHeader saveGameHeader = new SaveGameHeader(stream);
                if (saveGameHeader.IsSameVersion(Xenocide.CurrentVersion))
                {
                    // get the game state
                    BinaryFormatter formatter = new BinaryFormatter();
                    return (GameState)formatter.Deserialize(stream);
                }
                else
                {
                    Util.ShowMessageBox(Strings.SCREEN_LOADSAVEGAME_VERSION_CONFLICT);
                    return null;
                }
            }
'@
$c = $c.Replace($oldBlock, $newBlock)

# Fix WriteToFile
$c = $c.Replace(
    'using (StorageContainer container = GetContainer())
                {
                    string filename = Path.Combine(container.Path, saveName);',
    'string saveDir = GetSaveDirectory();
                string filename = Path.Combine(saveDir, saveName);'
)

# But this leaves a } in the wrong place. Fix it:
$c = $c.Replace(
    @"                {
                    string filename = Path.Combine(saveDir, saveName);
                    using (FileStream stream = File.Create(filename))
                    {
                        SaveGameHeader.WriteHeader(stream);
                        WriteGameState(stream);
                    }
                }",
    @"                string filename = Path.Combine(saveDir, saveName);
                    using (FileStream stream = File.Create(filename))
                    {
                        SaveGameHeader.WriteHeader(stream);
                        WriteGameState(stream);
                    }"
)

# Fix SaveGameExists
$c = $c.Replace(
    'using (StorageContainer container = GetContainer())
            {
                return File.Exists(Path.Combine(container.Path, filename));',
    'string saveDir = GetSaveDirectory();
            return File.Exists(Path.Combine(saveDir, filename));'
)

# Fix DeleteSaveGameFile
$oldBlock = @'
        private void DeleteSaveGameFile(string filename)
        {
            using (StorageContainer container = GetContainer())
            {
                string path = Path.Combine(container.Path, filename);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
'@
$newBlock = @'
        private void DeleteSaveGameFile(string filename)
        {
            string saveDir = GetSaveDirectory();
            string path = Path.Combine(saveDir, filename);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
'@
$c = $c.Replace($oldBlock, $newBlock)

# Replace GetContainer with GetSaveDirectory
$oldBlock = @'
        private StorageContainer GetContainer()
        {
            // this bit is dummy on windows
            IAsyncResult result = Guide.BeginShowStorageDeviceSelector(PlayerIndex.One, null, null);
            StorageDevice device = Guide.EndShowStorageDeviceSelector(result);
            // Now open container(directory)
            return device.OpenContainer(savesDirectory);
        }
'@
$newBlock = @'
        private string GetSaveDirectory()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, savesDirectory);
        }
'@
$c = $c.Replace($oldBlock, $newBlock)

[System.IO.File]::WriteAllText($f, $c, [System.Text.Encoding]::UTF8)
Write-Host "5. Fixed LoadSaveGameScreen.cs"

Write-Host "`nStorage files fixed!"
