<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN"
    "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">

<head>
    <meta http-equiv="Content-Type" content="text/html; charset=us-ascii" />

    <title>Project Xenocide - Contact form</title>
    <link href="xenocide.css" rel="stylesheet" type="text/css" />
</head>

<body>

    <div id="container">
        <div id="logo"></div>

        <div id="menu">
            <a href="index.html" id="menu-home"></a> <img src="images/menubreak.gif" alt="" />
			<a href="downloads.html" id="menu-downloads"></a> <img src="images/menubreak.gif" alt="" /> 
			<a href="http://docs.projectxenocide.com/index.php/General:FAQ" id="menu-faq"></a> <img src="images/menubreak.gif" alt="" /> 
			<a href="http://docs.projectxenocide.com/index.php/General:Recruiting_Process" id="menu-join"></a> <img src="images/menubreak.gif" alt="" /> 
			<a href="http://www.xcomufo.com/forums/index.php?showforum=258" id="menu-forum"></a> <img src="images/menubreak.gif" alt="" /> 
			<a href="contact.php" id="menu-contact"></a> <img src="images/menubreak.gif" alt="" /> 
			<a href="art_recent_add.html" id="menu-gallery"></a>
        </div>

        <div id="status">
            <div id="status-header">
                <a href="#">archive</a>
            </div>

            <div id="status-body"></div>

            <div id="status-build">	  <a href="http://www.projectxenocide.com/download/xna.Xenocide.0.4.0.1835.zip">Progress Release 0.4.0.1835</a><br />	  04 Apr 2008</div>
        </div>

        <div id="bar">
            <div id="image">
                <span>image</span>
            </div>

            <div id="login">
                <div id="login-body">
                    username:
                    <br />
                    <input name="username" type="text" class="text" />
                    <br />
                    password:
                    <br />
                    <input name="password" type="password" class="text" />
                    <br />
                    &nbsp;
                    <br />
                    <input name="submit" type="button" value="log in" class="submit" /> &nbsp; <a href="#">lost password?</a>
                </div>

                <div id="login-bottom">
                    <span>1</span>
                </div>

                <div id="login-corner">
                    <span>1</span>
                </div>
            </div>

            <div id="paypal">
                <form action="https://www.paypal.com/cgi-bin/webscr" method="post">
                    <input type="hidden" name="cmd" value="_xclick" /> 
					<input type="hidden" name="business" value="micahdg@xcomufo.com" /> 
					<input type="hidden" name="item_name" value="Project Xenocide" /> 
					<input type="hidden" name="item_number" value="micahdg" /> 
					<input type="image" src="./images/paypal.gif" name="I1" 
					 alt="Make payments with PayPal - it's fast, free and secure!" />
                </form>
            </div>
        </div>

        <div id="main">
            <div id="h-bar"></div>

            <div id="body">
                <div id="news-text">
                    Leave a message after the beep.
                </div>

                <form action="contact.php" method="post">
                    <div id="news-title">
                        Name:
                    </div>

                    <div id="news-text">
                        <input type="text" name="omgname" class="text" />
                    </div>

                    <div id="news-title">
                        E-mail:
                    </div>

                    <div id="news-text">
                        <input type="text" name="omgmail" class="text" />
                    </div>

                    <div id="news-title">
                        Text:
                    </div>

                    <div id="news-text">
                        <textarea name="omgmsg" class="textbox"></textarea>
                    </div>

                    <div id="news-title">
                        <input name="action" type="submit" value="send" class="submit" />
                    </div>
                </form>
				
<?php

function isNotValidEmail($email)
{
    $pattern = "^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,3})$";

    if (eregi($pattern, $email))
    {
        return false;
    }
    else
    {
        return true;
    }
}

$omgname = $_POST[omgname];
$omgmail = $_POST[omgmail];
$omgmsg = $_POST[omgmsg];

if ($_POST[action] == 'send')
{
    if ($omgname == '' && $omgmail == '' && $omgmsg == '')
       echo '<div id="news-title">Umm... have you considered, oh, I don\'t know... filling out the form?</div>';
    else
       if ($omgname == '')
           echo '<div id="news-title">We don\'t take nameless people, it makes it harder to mention them.</div>';
       else
          if ($omgmail == '')
             echo '<div id="news-title">No e-mail, no contact.</div>';
          else
             if ($omgmsg == '')
                echo '<div id="news-title">Yeah... if you wanna contact us, how about telling us why?</div>';
             else
                if (isNotValidEmail($omgmail))
                   echo '<div id="news-title">Who are you trying to kid? That\'s not a valid e-mail address.</div>';
                else
                {
                  mail('mad@projectxenocide.com', 'Xenocide Email Contact Request', htmlspecialchars($omgmsg), "From: {$omgname}, {$omgmail}");
                  echo "<div id=\"news-title\">E-mail sent. Thank you $omgname for contacting us!</div>";
                }
}
?>
            </div>

            <div id="footer">
                <div id="footer-left">
                    <span>1</span>
                </div>

                <div id="footer-body">
                    &copy; 2005 Project Xenocide
                </div>

                <div id="footer-right">
                    <span>1</span>
                </div>
            </div>
        </div>
    </div>

</body>

</html>
