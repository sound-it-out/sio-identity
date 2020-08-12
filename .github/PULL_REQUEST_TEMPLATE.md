## Pull Request template
Please, go through these steps before you submit a PR.

1. Make sure that your PR is not a duplicate.
2. If not, then make sure that:

    a. You have done your changes in a separate branch. Branches MUST have descriptive names that start with either the `bugfix/` or `feature/` prefixes. Good examples are: `bugfix/signin-issue` or `feature/issue-templates`.

    b. You have a descriptive commit message with a short title (first line).

    c. Tests for the changes have been added.

    d. Tests pass and do not throw any errors. If they do, fix them first and amend your commit (`git commit --amend`).

    e. Docs have been added/updated

3. **After** these steps, you're ready to open a pull request.

    a. Your pull request **MUST** target the `master` branch on this repository.

    b. Give a descriptive title to your PR.

    c. Provide a description of your changes:

      * What kind of change does this PR introduce? (Bug fix, feature, docs update, ...)

      * What is the current behaviour? (You can also link to an open issue here)
  
      * What is the new behaviour (if this is a feature change)?
  
      * Does this PR introduce a breaking change? (What changes might users need to make in their application due to this PR?)

    d. Put `closes #<Id>`, or `closes AB#<Id>` in your comment to auto-close the issue that your PR fixes.

**PLEASE REMOVE THE TEMPLATE ABOVE AND FILL OUT BELOW BEFORE SUBMITTING**

### What kind of change does this PR introduce? (Bug fix, feature, docs update, ...)

### What is the current behaviour? (You can also link to an open issue here)

### What is the new behaviour (if this is a feature change)?

### Does this PR introduce a breaking change? (What changes might users need to make in their application due to this PR?)