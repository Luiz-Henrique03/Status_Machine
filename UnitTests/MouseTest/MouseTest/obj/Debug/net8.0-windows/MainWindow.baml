   M S B A M L   `   `   `  ����D  @MouseTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:MouseTest	MouseTest  5   	       MouseTest.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   )localclr-namespace:MouseTest     5   	     � Title$  
MainWindow��5   	   $	��461��6   $	��543��6)    � WindowStyle$
 None=�5	   	    � 
ResizeMode$ NoResize=�6   .��5
      � -   5         �Name$ gridMouseArea��5
      
�������.+�5   
   �� -   5         ��Name$ Lbl_Main��5      $"��Passe o mouse na área cinza��6"   $	��Top=�6H   $��53��6`   $$��Bahnschrift SemiBold Condensed)�6l   $��14(�6�   $
��Bold"�6�   $
��Left=�6�   $	��448��6�   $��	21,32,0,0q�6�   �� 5   
   -   5         ��Name$ Cursor_Check��5      $
��Left=�6*   $��77��6E   $��194,123,0,0q�6Q   $	��Top=�6f   $	��143��6~   ��5      �� 5       �� Color$
 Gray��6"    � Triggers 5      :� 5       :� RoutedEvent$ 
MouseEnter��6   .[�5      �� .��5      �� .��5      �� 	 �� TargetProperty$.	 ((Rectangle.Fill).(SolidColorBrush.Color)�6-   	
 �� To	
 Green6r    g� Duration$ 0:0:0.5D�6}   5      5      5      5      �� 5   
   -   5        $ Left_Click_Check��5      $
��Left=�6.   $��74��6I   $��
92,261,0,0q�6U   
���   �$	��Top=�6x   $	��138��6�   $y�Hidden=�6�   ��5      �� 5      $
 Gray��6"    5      :� 5       $ MouseLeftButtonDown��6   .[�5!      �� .��5"      �� .��5#      �� $.	 ((Rectangle.Fill).(SolidColorBrush.Color)�6-   	
 Green6r   $ 0:0:0.5D�6}   5$      5%      5&      5(      �� 5*   
   -   5        $ Right_Click_Check��5*      $
��Left=�6/   $��74��6J   $��331,261,0,0q�6V   
���   �$	��Top=�6z   $	��138��6�   $y�Hidden=�6�   ��5+      �� 5,      $
 Gray��6"    5.      :� 5/      $ MouseRightButtonDown��6   .[�50      �� .��51      �� .��52      �� $.	 ((Rectangle.Fill).(SolidColorBrush.Color)�6-   	
 Green6r   $ 0:0:0.5D�6}   53      54      55      57      �� 58   
   -   5         ��Name$ 
Img_result��58      $
��Left=�6$   $	��161��6?   $��175,162,0,0q�6L   $	��Top=�6a   $	��178��6y    J� RenderTransformOrigin$ 0.582,0.475&�6�   $��/Errors.png��6�   $y�Hidden=�6�   �� 59   
   -   5         ��Name$ Btn_Try_Again��59      $��Tentar Novamente��6(   $��Center=�6C   $��	0,369,0,0q�6`   $	��Top=�6s   $��46��6�   $	��173��6�   $$��Bahnschrift SemiBold Condensed)�6�   $y�Hidden=�6�   5:      5;      