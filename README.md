# Moneybox Money Withdrawal

The solution contains a .NET core library (Moneybox.App) which is structured into the following 3 folders:

* Domain - this contains the domain models for a user and an account, and a notification service.
* Features - this contains two operations, one which is implemented (transfer money) and another which isn't (withdraw money)
* DataAccess - this contains a repository for retrieving and saving an account (and the nested user it belongs to)

## The task

The task is to implement a money withdrawal in the WithdrawMoney.Execute(...) method in the features folder. For consistency, the logic should be the same as the TransferMoney.Execute(...) method i.e. notifications for low funds and exceptions where the operation is not possible. 

As part of this process however, you should look to refactor some of the code in the TransferMoney.Execute(...) method into the domain models, and make these models less susceptible to misuse. We're looking to make our domain models rich in behaviour and much more than just plain old objects, however we don't want any data persistance operations (i.e. data access repositories) to bleed into our domain. This should simplify the task of implementing WithdrawMoney.Execute(...).

## Guidelines

* The test should take about an hour to complete, although there is no strict time limit
* You should fork or copy this repository into your own public repository (Github, BitBucket etc.) before you do your work
* Your solution must build and any tests must pass
* You should not alter the notification service or the the account repository interfaces
* You may add unit/integration tests using a test framework (and/or mocking framework) of your choice
* You may edit this README.md if you want to give more details around your work (e.g. why you have done something a particular way, or anything else you would look to do but didn't have time)

Once you have completed test, zip up your solution, excluding any build artifacts to reduce the size, and email it back to our recruitment team.

Good luck!


## Developer Notes

I created a new feature branch called `feature/withdrawal`, where I implemented the feature and ran the tests before merging it into the `master` branch. I have commited the changes incrementally, with each commit representing a logical step in the development process.

### Unit Tests

The solution contains a number of unit tests which are located in the Moneybox.App.Tests project. These tests cover the existing functionality of the TransferMoney and the implemented WithdrawMoney features. I took the TDD approach, creating the Unit Tests first then developing the features with the Unit Tests in mind.
I used the NUnit test framework for the unit tests, and Moq for mocking dependencies. The tests are structured to cover various scenarios including successful transfers, insufficient funds, and low balance notifications to prevent misuse of the domain models and the TransferMoney.Execute logic I had to base off.
With Autofixture, I was able to easily create test data for the domain models. This may have been overkill for this task, but it allowed me to focus on the logic rather than the data setup. I am also more familiar with it, so I used it to speed up the test creation process.

### Refactoring

I refactored the TransferMoney.Execute method to move the logic into the domain models, specifically the Account class. The Account class now has methods for transferring and withdrawing money, which handle the necessary checks and notifications internally, preventing misuse of the domain model. This reduces the complextity of the Execute methods and allows for better encapsulation of the business logic.

### What I would do next
If I had more time, I would:
* Implement more comprehensive integration tests to ensure the entire flow works as expected, including the interaction between the domain models and the data access layer.
* Add more unit tests to cover edge cases and ensure the robustness of the domain models. For example, prevent transfer from/to same account, or transferring more than the available balance.
* Configure the GitHub Actions to run the tests automatically on each push or pull request, ensuring that the code remains stable and that any changes are validated.