# bandManager
This idea was on the back burner for quite a while, and I never had the time to pursue this, until last week. I finally sat down and developed a Microsoft Band customization app, that helps you take advantage of the Band SDK and customize the BandTheme. I know the Microsoft Health app allows you to customize the BandTheme using various styles, however, I felt it to be limited in terms of granular customization, which inspired me to develop this app, which helps you to customize each and every Band Color class. Another thing I felt the Microsoft Health app lacked in terms of Band personalization was the visual UI that helped the user interpret how the theme actually affected the Band Tiles. This UWP application actually helps the user have a better understanding towards which parts of the Band are being customized.
This post will help you guide through the Band SDK functions used to retrieve and update the BandTheme. There are 8 color classes that exist on the Band, however, only 6 of the 8 can be customized using the Band SDK.
```
• Base
• High Contrast
• Lowlight
• Highlight
• Muted
• Secondary
```


# Screenshots
![Alt text](/screenshots/Connecting_to_the_Band.jpg?raw=true "Connecting to Microsoft Band")
![Alt text](/screenshots/Theme_Retrieved.jpg?raw=true "Connected and successfully retrieved the BandTheme")
![Alt text](/screenshots/Personalizing_2a_1.jpg?raw=true "Choosing color using ComboBox")
![Alt text](/screenshots/Personalizing_2a_2.jpg?raw=true "Choosing color using ComboBox")
![Alt text](/screenshots/Personalizing_2b_1.jpg?raw=true "Choosing color using hex value")
![Alt text](/screenshots/Personalizing_2b_2.jpg?raw=true "Choosing color using hex value")
![Alt text](/screenshots/Upload_Theme_1.jpg?raw=true "Uploading theme to Band")
![Alt text](/screenshots/Upload_Theme_2.jpg?raw=true "Upload completed")
  
   
    

# Full detailed guide
Refer to this blog post for complete detailed guide along with video tutorial.  
http://keerats.com/blog/2016/customizing-microsoft-band-using-band-sdk-uwp/
