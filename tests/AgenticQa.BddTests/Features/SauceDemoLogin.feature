Feature: Sauce Demo login

@ui @saucedemo @login
Scenario Outline: Login with accepted user and validate page title
  Given the Sauce Demo login page URL is configured
  When the user logs in to Sauce Demo as "<username>"
  Then the Sauce Demo page title should be "Swag Labs"

Examples:
  | username                  |
  | standard_user             |
  | locked_out_user           |
  | problem_user              |
  | performance_glitch_user   |
  | error_user                |
  | visual_user               |

@ui @saucedemo @login
Scenario Outline: Successful users should land on inventory page
  Given the Sauce Demo login page URL is configured
  When the user logs in to Sauce Demo as "<username>"
  Then the user should land on the inventory page

Examples:
  | username                |
  | standard_user           |
  | problem_user            |
  | performance_glitch_user |
  | error_user              |
  | visual_user             |

@ui @saucedemo @login
Scenario: Locked out user should see locked-out error message
  Given the Sauce Demo login page URL is configured
  When the user logs in to Sauce Demo as "locked_out_user"
  Then the Sauce Demo error message should be "Epic sadface: Sorry, this user has been locked out."
