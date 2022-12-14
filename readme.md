## Contest Killer 使用说明

###  关于 Contest Killer

- 基于 .NET，WPF 框架开发的**信息学竞赛离线测评器**

- CCR - Plus 换皮版（测评器内核使用 CCR - Plus 测评器源码）

- 适合较大屏幕使用

### 文件结构

Contest Killer 采用和 **CCR - Plus 相似的竞赛文件结构**，在 CCR - Plus 上测评的竞赛经过较小改动，便可以直接在 Contest Kiler 上打开，具体如下：

- 竞赛根目录下两个**固定名称的目录**：`data`，`src`，其中`data`里存放题目测试数据，`src`存放选手源代码数据
- 每个题目的测试数据存放至`data`目录的**名称和题目名称相同的子目录下**，输入输出数据不单独创建文件夹区分
- 每个选手的源代码按照`题目名称 + 后缀名`的方式存放至`src`目录的**名称和选手名称相同的子目录下**，Contest Killer 仅支持`C++`，`C#`，`Java`，`Python` 四种语言，对应的后缀名为`.cpp`，`.cs`，`.java`，`.py`
- Contest Killer 会在竞赛根目录创建一个配置文件 `.ckconfig`，保存竞赛的所有信息，**建议不要擅自修改或删除**，有可能导致不能打开竞赛或者竞赛内容错误
- 如：某次竞赛，名称为 20200818_Contest，有 2 位选手 PlayerA，PlayerB，有 2 道题目 ProblemA, ProblemB，使用的语言均为 C++，则对应的文件结构如下所示：

```
20220818_Contest
	- src
		- PlayerA
			- ProblemA.cpp
			- ProblemB.cpp
		- PlayerB
			- ProblemA.cpp
			- ProblemB.cpp
	- data
		- ProblemA
			- 1.in
			- 1.out
			- ...
		- ProblemB
			- 1.in
			- 1.out
			- ...
	- .ckconfig
```



### 配置竞赛

#### 竞赛简要信息

打开竞赛时请选择根目录打开，然后可以在 “概览” 页面编辑竞赛简要信息（名称，描述），竞赛创建时间不能更改

#### 试题

**添加：**在竞赛目录下的`data`目录里新建一个文件夹，名称和新试题的名称一致，将输入输出文件全部放入，然后在 “试题” 页面点击 “刷新” 按钮即可导入新试题

**修改：**

- 某一试题上右键菜单 “编辑”，进入试题编辑页面
- 编辑页面可以配置试题的时空限制，额外的编译命令行参数，使用的语言，**试题名称同文件夹名称一致，不可更改**
- 时间限制为整数，单位为毫秒 ms，空间限制为是浮点数，单位为兆字节 MB
- 试题的输入和输出文件为 **“试题名称 + 后缀名” 且不可更改**，后缀名取决于使用的语言
- 对于多个测试点可以将其合并，**合并后的测试点全部数据均正确才能得分**，包含多个数据的点可以被拆分为只包含单个数据的测试点
- 可以通过先合并再拆分的方式来批量修改数据点的分数
- **添加 / 删除 测试点**：将新的测试文件放入试题目录下，刷新试题，新的测试文件将自动识别为新的测试点，自动识别的测试点只包含单个文件，删除测试点同理

**删除：**删除试题时会将其对应的目录和全部测试文件一并删除，请谨慎操作

#### 评测

几点注意事项：

- **在 “评测” 页面进行评测**，“概览“ 页面的成绩列表不能修改（观赏用）
- **开始评测后，将不能切换到 ”概览“ 和 ”试题页面“ 直至评测结束**
- 评测**采用队列方式**，除非手动停止，当评测队列为空时才会停止评测

开始评测后， 切换到 ”评测 - 任务“  页面，上方会显示评测信息，如：

```
1/4    PlayerA - ProblemA 1/10    0ms 3.00MB Accepted
```

- `1/4`：评测进度，一个评测任务为一位选手对应一道试题（全部评测时将产生 选手数 × 试题数 的评测任务），当某位选手的某一试题所有测试点全部测评完成时，该评测任务完成
- `PlayerA - ProblemA`：测评的选手和题目名称
- `1/10`：对应题目评测进度，显示正在测评的测试点编号和总测试点数
- `0ms 3.00MB Accepted`：测评结果
- “任务” 页面下方会显示每个测试点详细的测评信息

**评测结束后**，在选手的分数按钮上右键点击 “查看” 可以查看该选手所有试题的详细测评信息和导出成绩至HTML



### 设置

#### 外观

- tab 宽度：左侧导航栏的宽度
  - 自动：根据显示文字的长度自动调整，不会省略文字
  - 固定：超出固定宽度的文字将被省略
- 背景图片：设定应用程序的背景图片
  - 启用：可以从本地或者内置图片中选择，不受暗黑模式影响
  - 不启用：显示纯色背景，受暗黑模式影响
  - **内存释放：**由于使用 BitmapImage 加载图片（而且还是 Uri 定位的方式而不是 Stream），以下情况下应用程序的内存占用会快速上升到非常高的水平（可达 1GB）：
    - 点开 “加载图片” 并且选择了 “使用默认图片”，加载了内置了图片时（内置图片分辨率均为 4K，占用内存大）
    - 频繁更换图片时
  - 目前还没有想到有效的方式释放内存，**强烈建议不要频繁更换图片，且每次更换后手动重启应用来释放内存**
- 主题颜色和暗黑模式：
  - 可以在 MD 调色板 给定的颜色中或者颜色选取器中选取主题颜色
  - **对比度和暗黑模式：**手动设置的主题色**仅对明亮模式准确生效**，在切换到暗黑模式时，主题颜色会自动调整，调整后的主题色和设定颜色的差别取决于对比度的设置
- 语言：语言选择，需要重启应用

#### 默认值

- 时空限制：设置默认的时空限制，在导入新试题时用到

- 测试点分数：设置默认的测试点分数，在导入新的测试点时用到

- 编译器和解释器配置：设置默认的编译器，编译命令行参数，解释器路径

  - **注意：编译器和解释器都是绝对路径，不是环境变量或者相对路径**

  - 约定：`{{fileName}}` 表示源文件名（不含路径），`{{exeName}}` 表示可执行文件名（不含路径），其中Java 对应 `.class`，C# 和 C++ 对应 `.exe`，`{{fileNamePre}}` 对应源文件名（不包含扩展名和路径）

  - C++

    - 编译器路径为 `g++.exe` 路径，若安装了 DevC++ 可以直接用 `MinGW` 里面的
    - 编译时，命令为 `g++.exe {源文件名} {默认编译参数} {试题额外参数}`
    - 默认编译参数必须包含 `-o {{exeName}}`，用于生成 exe 可执行文件

  - C#

    - 编译器路径为 `csc.exe` 路径
    - 编译时，命令为 `csc.exe {源文件名} {默认编译参数} {试题额外参数}`

  - Java

    - 编译器路径为 `javac.exe` 路径
    - 解释器路径为 `java.exe` 路径
    - 同时安装了 `jre` 和 `jdk` 请使用`jdk`里面的 `java.exe` 和 `javac.exe`
    - 编译时，命令为 `javac.exe {源文件名}`
    - 解释器命令为`java.exe {类名}`

  - Python

    - 解释器路径为 `python` 路径

    - Python 文件不预先编译，试题的额外命令行参数无效
    - 解释器命令为 `python {源文件名}`
