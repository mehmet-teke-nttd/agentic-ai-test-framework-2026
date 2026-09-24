Feature: Mars page validation

@ui @smoke @mars
Scenario: Navigate to Mars demo page and validate title
  Given the Mars demo page URL is configured
  When the user navigates to the Mars demo page
  Then the page title should be "Mars Commuter: Travel to Mars for Work or Pleasure!"
