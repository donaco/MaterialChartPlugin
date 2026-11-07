# MaterialChartPlugin

[![Release](https://img.shields.io/github/release/donaco/MaterialChartPlugin.svg?style=flat-square)](https://github.com/donaco/MaterialChartPlugin/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/donaco/MaterialChartPlugin/latest/total.svg?style=flat-square)](https://github.com/donaco/MaterialChartPlugin/releases/latest)

資材のグラフを表示するKanColleViewer用プラグインです。

### 機能
* 資材データの自動記録
* 資材・高速修復材・高速建造材のグラフ表示
* .csv形式での資材データのエクスポート
* ポップアップウィンドウでのグラフ表示

### インストール
* KanColleViewer 4.7.0 以降が必要です。
* 圧縮ファイルを展開し、PluginsフォルダとDLLファイルをKanColleViewer にコピーしてください。

### 削除方法
* KanColleViewer を終了し、PluginsフォルダとDLLファイルを削除してください。
* 設定と資源履歴の保存先は `%LOCALAPPDATA%\grabacr.net\KanColleViewer\Plugins\MaterialChartPlugin\` です。必要に応じて削除してください。
* 旧 `%APPDATA%\terry_u16\MaterialChartPlugin\Settings.xaml` と旧 `%LOCALAPPDATA%\terry_u16\MaterialChartPlugin\` の設定・履歴は自動で引き継ぎます。
* 旧データを削除したい場合は、設定>ブラウザ操作>旧Verのデータを削除する で削除できます。移行結果を確認してから必要に応じて削除してください。
* 新保存先のファイルを削除した場合、旧ファイルが残っていると再取り込みされます。

### ライセンス
* [The MIT License (MIT)](LICENSE.txt)

### ダウンロード
[![Release](https://img.shields.io/github/release/donaco/MaterialChartPlugin.svg?style=flat-square)](https://github.com/donaco/MaterialChartPlugin/releases/latest) [MaterialChartPlugin - GitHub Releases Page](https://github.com/donaco/MaterialChartPlugin/releases/latest)


### 使用ライブラリ
以下のライブラリを使用して開発を行っています。

#### [KanColleViewer](https://github.com/Grabacr07/KanColleViewer)
> The MIT License (MIT)
> 
> Copyright (c) 2013 Grabacr07
* **ライセンス :** The MIT License (MIT)
* **ライセンス全文 :** [licenses/KanColleViewer.txt](licenses/KanColleViewer.txt)

#### [CommunityToolkit.Mvvm](https://github.com/Grabacr07/MetroRadiance)
* **用途 :** ViewModel の変更通知 (`ObservableObject`)
* **ライセンス :** The MIT License (MIT)
* **ライセンス全文 :** [https://licenses.nuget.org/MIT](https://licenses.nuget.org/MIT)

#### [System.ComponentModel.Composition](https://www.nuget.org/packages/System.ComponentModel.Composition/9.0.0)
* **用途 :** MEF によるプラグイン公開
* **ライセンス :** The MIT License (MIT)
* **ライセンス全文 :** [https://licenses.nuget.org/MIT](https://licenses.nuget.org/MIT)

### [System.Reactive](https://www.nuget.org/packages/System.Reactive/6.1.0)
* **用途 :** 非同期処理
* **ライセンス :** The MIT License (MIT)
* **ライセンス全文 :** [licenses/System.Reactive.txt](licenses/System.Reactive.txt)

#### [Silverlight/WPF Data Visualization](http://dlaa.me/blog/post/9999344)
* **用途 :** `System.Windows.Controls.DataVisualization.Toolkit.dll` によるグラフ表示
> Copyright (c) 2010 David Anson
* **ライセンス :** The MIT License (MIT)
* **ライセンス全文 :** [licenses/SilverlightWPFDataVisualization.txt](licenses/SilverlightWPFDataVisualization.txt)