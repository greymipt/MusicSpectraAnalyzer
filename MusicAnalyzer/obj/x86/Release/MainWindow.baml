   M S B A M L   `   `   `  ����   MusicAnalyzer     WpfApplication1.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     � Title$  
MainWindow��$	��400��$	��650��$	��400��$	��650��.���� -   $��tabControl1��$��Stretch=�$��Stretch=�}�.  $��0q�$	��300��$	��500��$��0q�$��0q�.@��� -   $��tabItem1��$��
Find Music��$��
-3,-3,-2,0q�$��0q�.��� $��Stretch=�$��Stretch=�$��0q� � RowDefinitions �� $��* �$	��100�� � ColumnDefinitions �� $	��230 ��� $��* �$	��350��.+��� $��1��$��0��
��������� $��0��
��������� -   $��textBox1��$��23��$
��Left=�$��	30,34,0,0q�$	��Top=�$	��171��$��1q�$��1��.����O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem �� 6sys&clr-namespace:System;assembly=mscorlib      �� 
TargetType" �� �� 	Resources    CueBannerBrush j� 
AlignmentX j� 
AlignmentY j� Stretch !� Visual��%`   &	        !� $
 Left=�$ Center=�$
 None=� �� $��Search��
�������$��Italic$�	 �� Triggers	 P� 
 P� Property
 � �� �� Empty P� Value# Z . ���  �� Property � �� �� Value# [  P� 
 � �� Z� . ���  � ��# [  P� 
 �@J�IsKeyboardFocused . . ���  � ��
 �����.��&   System.Windows.Controls.DataGrid  -   $��	dataGrid1��   ColumnWidth5  /System.Windows.Controls.DataGridLengthConverter$ *    AutoGenerateColumns . $��1��$��Stretch=�$��30,70,20,20q�$��Stretch=�   GridLinesVisibility$
 None=� � ToolTip$A ;Shows music from database. Right-click on item to see menu.������  � ContextMenu �� .@�m� -   $
��Play��m� -   $��Find Similar Music��m� -   $��Open Conataining Folder���� -	   $��	comboBox1��$��0��$��23��$
��Left=�$��	25,34,0,0q�$	��Top=�$��45��$S MNumber of songs to show. Both for similar songs search and search by spectra.���� -
   $��	comboBox2��$��0��$��23��$
��Left=�$��	92,34,0,0q�$	��Top=�$	��111��$. (Choose algorithm to search similar songs���� -   $��button5��$��Save Changes ��$��23��$��Right=�$��	0,34,20,0q�$	��Top=�$
��Auto��$ Save changes to library��$��1���� -   $��label1��$#��Search similar music Options ��$��28��$
��Left=�$��33,2,0,0q�$	��Top=��� -   $��
rectangle2��$��2��$
��Left=�$��	230,0,0,0q�
�������$��Stretch=�$��3��
��������� -   $��border1��
�������$��2q�$
��Left=�$��	16,99,0,0q�$	��Top=�$	��200�� �� CornerRadius$ 10������  �� ElementName$	 btn�� �� Path$ ActualHeight�._�� $��Stretch=�$��Stretch=�.+��� -   $��	checkBox2��$��SubBass��$��16��$
��Left=�$��	12,20,0,0q�$	��Top=�$ 16-60 Hz���� -   $��	checkBox3��$
��Bass��$��16��$
��Left=�$��	12,50,0,0q�$	��Top=�$ 	60-250 Hz���� -   $��	checkBox4��$	��Mid��$��16��$
��Left=�$��	12,80,0,0q�$	��Top=�$ 250-2000 Hz���� -   $��	checkBox5��$��HighMid��$��16��$
��Left=�$��
12,110,0,0q�$	��Top=�$ 2000-6000 Hz���� -   $��	checkBox6��$
��High��$��16��$
��Left=�$��
12,140,0,0q�$	��Top=�$ 6000-15000 Hz��	� -   $��radioButton1�� 	� 	GroupName$ SBass��$	��Min��$��16��$
��Left=�$��	95,20,0,0q�$	��Top=�{�.   d� 	IsChecked$
 TrueY�	� -   $��radioButton2��$ SBass��$	��Max��$��16��$
��Left=�$��
142,20,0,0q�$	��Top=�{�.  	� -   $��radioButton3��$
 Bass��$	��Min��$��16��$
��Left=�$��	95,50,0,0q�$	��Top=�{�.  $
 TrueY�	� -   $��radioButton4��$
 Bass��$	��Max��$��16��$
��Left=�$��
142,50,0,0q�$	��Top=�{�.  	� -   $��radioButton5��$	 Mid��$	��Min��$��16��$
��Left=�$��	95,80,0,0q�$	��Top=�{�.  $
 TrueY�	� -   $��radioButton6��$	 Mid��$	��Max��$��16��$
��Left=�$��
142,80,0,0q�$	��Top=�{�.  	� -   $��radioButton7��$
 HMid��$	��Min��$��16��$
��Left=�$��
95,110,0,0q�$	��Top=�{�.  $
 TrueY�	� -   $��radioButton8��$
 HMid��$	��Max��$��16��$
��Left=�$��142,110,0,0q�$	��Top=�{�.  	� -   $��radioButton9��$
 High��$	��Min��$��16��$
��Left=�$��
95,140,0,0q�$	��Top=�{�.  $
 TrueY�	� -   $��radioButton10��$
 High��$	��Max��$��16��$
��Left=�$��142,140,0,0q�$	��Top=�{�.  �� -   $��button2��$��Filter��$��28��$
��Left=�$��
57,179,0,0q�$	��Top=�$��74��$��16(�$&  Filter songs by frequency range.���� -   $��	checkBox7��$��Filter music by spectra��$��16��$
��Left=�$��	30,77,0,0q�$	��Top=��� -    $��label3��$��Label��$��1��$��28��$
��Left=�$��30,2,0,0q�$	��Top=��� 
�������$��2,3,2,2q�$��2��$��Stretch=�$��0,0,0,0q�$��Stretch=� K� ZIndex$ 0�� J� 	AllowDrop .  �� -!   $��tabItem2��$��Music Library��$��
-2,-3,-2,0q�.��� $��Stretch=� �� $��* � �� $	��120 ��� $��* �.+��� $��0��$��0��
��������� $��1��$��0��
��������� -"   $��button1��$= 7Process music files in folder and save info to DataBase��$��
Add Folder��$��23��$
��Left=�$��	20,46,0,0q�$	��Top=�$��75���� -#   $��listBox2��$��1��$��0��$��Stretch=�$��50,70,50,50q�$��Stretch=�� -$   $��progressBar1��$��1��$��15��$��Stretch=�$��50,45,50,20q�$	��Top=��� -%   $��label2��$��Label��$��1��$
��Auto��$
��Left=�$��	50,10,0,0q�$	��Top=�$
��Auto���� -&   $��	checkBox1��$= 7Process files in parallel. Fast but CPU load is maximal��$��Parallel Mode��$��16��$
��Left=�$��
15,20,15,5q�$	��Top=�$ FalseY��� -'   $��button3��$* $Clears Library and creates empty one��$��Clear ��$��23��$
��Left=�$��	20,0,0,20q�$��Bottom=�$��75���� -(   $��button4��$��Stop ��$��23��$
��Left=�$��	20,87,0,0q�$	��Top=�$��75��$F @Stop processing files. Wait some time for action to take effect.���� -)   $��
rectangle3��$��2��$
��Left=�$��	120,0,0,0q�
�������$��Stretch=�$��3��
��������� 
�������$��2,3,2,2q�$��2��$��Stretch=�$��0,0,0,0q�$��Stretch=�