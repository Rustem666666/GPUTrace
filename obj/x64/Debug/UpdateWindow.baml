   M S B A M L   `   `   `  ����D  @GPU trace, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:GPU_trace	GPU_trace  5   	       GPU_trace.UpdateWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   )localclr-namespace:GPU_trace     5   	   -   5          � Title$  	GPU trace��5   	   $	��150��6   $	��250��6(    � 
ResizeMode$ NoResize=�5	   	    � ShowInTaskbar . $��0q�5
   	   
������  � WindowStartupLocation$ CenterScreen=�5   	    � AllowsTransparency .  � WindowStyle$
 None=�5   	   .��5      � .+�5   
   �� -   5        $��border��5      $
��auto��6   $
��auto��6,   
��� ���$��1q�6M    �� CornerRadius$ 
20,0,20,20��5      ��5      �� 5      ��5      � � 5       � Color$ 	#720E1F87��6+    � Offset$	 0.0Q�6=   6L   � 5      $ 	#723C491E��6+   $	 0.5Q�6=   6L   5      5      ._�5      � -   5        
������ 	 � RowDefinitions	 5      �� 5      $��25 �6$   �� 5      $��55 �6$   �� 5      $��50 �6$   �� 5      $��15 �6$   .+�5!      � -   5        $��ToolBar��5!      
������ $��0��6?   .+�5"      �� -   5        $��
tbFilename��5"       $��
GPU Z text��62   $��20(�5#   $   
���/���$��10,0,0,0q�5$   $   $��Center=�5%   $   $y�Hidden=�5&   $   �� 5'      
 �� Orientation$
 
Horizontal=�6!   $��Right=�6:   .+�5(      �� -   5        $��CloseButton��5(       $��15��63   $��15��6?   $��3,3,6,0q�6J   ��5)      �� 5*   "    �� 
TargetType" ��6(   .�5+   &   ��  �� Property � �� �� Value$ Source/close_noactive.png��6?    �� Triggers 5,   &   �� 5-   *    �� Binding 66   �� *IsMouseOver+ �� ElementName$ CloseButton�� �� Value true5.   .   .a�5/   .   ��  � ��$ Source/close_active.png��6G   50   +   52   #    J� Effect 54      -  'System.Windows.Media.Effects.BlurEffect  55   "      Radius$ 1Q�6-   57      58      59      � 5:      $��1��6   .+�5;      �� -   5        $��	tb_update��5;       $%��Вышло обновление��61   $��25(�5<       
���/���$��Center=�5=       $��Center=�6=   �� 5>      -   5        $��	tb_newver��5>       $4��.Новая версия 2.0, обновить?��61   $��12(�5?       
���/���$��Center=�5@       $��Bottom=�6=   5A      � 5B      $��2��6   .+�5C      �� $
 
Horizontal=�6!   .+�5D      �� -	   5        $��80��5D   !   $��30��6,   $
��Да��68   ���#5151
�������
���/���$��35,0,0,0q�5G   !   $��20(�5H   !   $��1q�6/   6C   �� 5J      -
   5        $��90��5J   !   $��30��6,   $��Нет��68   ���#5151
�������
���/���$��0,0,0,0q�5M   !   $��20(�5N   !   $��1q�6/   6C   5P      5Q      5R      5S      5T      5U      