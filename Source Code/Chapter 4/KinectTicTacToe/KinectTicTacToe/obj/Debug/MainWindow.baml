   M S B A M L   `   `   `  ����J  FKinectTicTacToe, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:KinectTicTacToeKinectTicTacToe  5   	        KinectTicTacToe.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   /localclr-namespace:KinectTicTacToe     5   	     � Title$  Kinect TicTacToe��5   	   $��375.836��6"   $	��330��63   .��5	      � $��	0,0,0,5.5q�6   .+�5
   
   � $
��Left=�6   $	��300��6*   $��	10,10,0,0q�67   $	��Top=�6J   $	��300��6b   $��3��6n    � RowDefinitions 5      �� 5      $	��100 �6    �� 5      $	��100 �6    �� 5      $	��100 �6     � ColumnDefinitions 5      �� 5      $	��100 �6#   �� 5      $	��100 �6#   �� 5      $	��100 �6#   .+�5      �� 
���   �$��0,0,2,2q�6)   ._�5      �� -   5         ��Name$ topLeft��5      $
��Left=�6-   $
��Wrap=�6H   $�� ��6\   $	��Top=�6d   $��48(�6|   $	��100��6�   $	��100��6�    �� TextAlignment$ Center=�6�   5      �� 5      
���   �$��2,0,2,2q�6)   $��1��6C   ._�5      �� -   5        $	 top��5      $
��Left=�6)   $
��Wrap=�6D   $�� ��6X   $	��Top=�6`   $��48(�6x   $	��100��6�   $	��100��6�   $ Center=�6�   5      �� 5      
���   �$��2,0,0,2q�6)   $��3��6C   ._�5      �� -   5        $ topRight��5      $
��Left=�6.   $
��Wrap=�6I   $�� ��6]   $	��Top=�6e   $��48(�6}   $	��100��6�   $	��100��6�   $ Center=�6�   5      �� 5      
���   �$��0,2,2,2q�6)   $��1��6C   ._�5      �� -   5        $
 left��5      $
��Left=�6*   $
��Wrap=�6E   $�� ��6Y   $	��Top=�6a   $��48(�6y   $	��100��6�   $	��100��6�   $ Center=�6�   5       �� 5!      
���   �$��2q�6)   $��1��6=   $��1��6J   ._�5"      �� -   5        $ center��5"      $
��Left=�6,   $
��Wrap=�6G   $�� ��6[   $	��Top=�6c   $��48(�6{   $	��100��6�   $	��100��6�   $ Center=�6�   5#      �� 5$      
���   �$��2,2,0,2q�6)   $��1��6C   $��2��6P   ._�5%      �� -   5        $ right��5%      $
��Left=�6+   $
��Wrap=�6F   $�� ��6Z   $	��Top=�6b   $��48(�6z   $	��100��6�   $	��100��6�   $ Center=�6�   5&      �� 5'      
���   �$��0,2,2,0q�6)   $��2��6C   ._�5(      �� -   5        $ 
bottomLeft��5(      $
��Left=�60   $
��Wrap=�6K   $�� ��6_   $	��Top=�6g   $��48(�6   $	��100��6�   $	��100��6�   $ Center=�6�   5)      �� 5*      
���   �$��2,2,2,0q�6)   $��2��6C   $��1��6P   ._�5+      �� -   5        $ bottom��5+      $
��Left=�6,   $
��Wrap=�6G   $�� ��6[   $	��Top=�6c   $��48(�6{   $	��100��6�   $	��100��6�   $ Center=�6�   5,      �� 5-      
���   �$��2,2,0,0q�6)   $��2��6C   $��2��6P   ._�5.      �� -	   5        $ bottomRight��5.      $
��Left=�61   $
��Wrap=�6L   $�� ��6`   $	��Top=�6h   $��48(�6�   $	��100��6�   $	��100��6�   $ Center=�6�   5/      50      �� 51   
   -
   5         ��Name$ 	resultbox��51      $
��Left=�6%   $��22��6@   $��
10,316,0,0q�6L    �� TextWrapping$
 Wrap=�6`   $)��#Say 'Start' to commence a new round��6t   $	��Top=�6�   $	��305��6�   53      54      