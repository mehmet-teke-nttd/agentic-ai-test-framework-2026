Feature: Login

@ui @smoke @login
Scenario: Successful login with valid credentials
  Given the user is on the login page
  When the user logs in with valid credentials
  Then the dashboard should be visible
  And the welcome message should be "Welcome"
