Feature: Login

@ui @smoke @login
Scenario: Successful login with valid credentials
  Given the user has a valid login test intent
  When the test intent is validated
  And the execution contract is generated
  And the execution contract is executed
  Then the overall test result should be PASSED
  And a result file should be written

@ui @login
Scenario: Login verification fails
  Given the user has a valid login test intent
  When the test intent is validated
  And the execution contract is generated
  And the execution contract is configured to force a verification failure
  And the execution contract is executed
  Then the overall test result should be FAILED
  And dependent steps should be skipped
  And a result file should be written

@ui @login
Scenario: Login execution is blocked by missing target
  Given the user has a valid login test intent
  When the test intent is validated
  And the execution contract is generated
  And the execution contract is configured with an unregistered target
  And the execution contract is executed
  Then the overall test result should be BLOCKED
  And dependent steps should be skipped
  And a result file should be written
