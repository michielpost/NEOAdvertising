# Neo Advertising

Publishers and Advertisers can use this smartcontract as an advertising marketplace.


## How does it work?

Publishers can create an advertisement spot with a minimum price.
Advertisers can place a bid for an ad on this adspace, the highest bid per day will be placed.

The SmartContract takes care of the payment. Before an advertiser can bid on advertisement, it has to deposit funds with the smart contract. The funds can be withdrawn at any time, unless it is used for an advertisement.
Publishers can withdraw their income at any time.
Payment for advertisement is done with GAS.

Currently only basic advertisements are supported. Advertisements contain text and an URL.

## Technology Stack
- NEO Blockchain (testnet)
- C# Smart Contract

## Smart Contract

The Smart Contract is written in C# and has multiple operations:

- Create
This operation creates a new ad space with a specified ID and minimum amount of GAS needed to place an ad
Parameters: operation name: 'create', owner address, AdId, minimum GAS
- Check Minimum Offer
Check the current offer for an ad space on the given date. This is the minimum amount of GAS needed to place an ad on this ad space
Parameters: operation name: 'minoffer', owner address, AdId, date
- Buy Ad Space
Place an ad. Submit text and an url and a date to place the advertisement on that day.
Parameters: operation name: 'buy', owner address, AdId, date, text, url
- Deposit
Deposit GAS which can be used to purchase/bid on ad placement.
Parameters: operation name: 'deposit', owner address
- Withdraw
Withdraw available GAS that is not locked by a bid and is not used to buy adspace.
Parameters: operation name: 'withdraw', owner address, date
- Get publisher profit
This is used by publishers to withdraw available GAS that you earned for a specified date
Parameters: operation name: 'profit', owner address, date

## Website

The website will provide an easy to use javascript interface for website publishers to place the advertisements on their website.
NOTE: Currently not yet implemented.

## Installation

- Open the solution file in Visual Studio 2017
- Restore NuGet packages
- Compile

#### SmartContract
- NeoAdvertisingContract.cs will be compiled to `src\NeoAdvertising.Contracts\bin\Debug\NeoAdvertisingContract.avm`
- Upload this to your private net or use the contract on the testnet

## Roadmap

- Website for promotion and easy to use javascript which  website publishers can include on their website
- Keeping track of the number of views for an advertisement
