using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.Numerics;

namespace NeoAdvertising.Contracts
{
  /// <summary>
  /// SmartContract for Text Advertisements
  /// Everybody can create an ad space
  /// You can bid on an ad space to place an advertisement. Highest bid will be shown for 1 day
  /// You have to have funds with the smart contract first, funds can be withdrawn if they are unused
  /// Owner of the adspace can withdraw income of the adspaces
  /// </summary>
  public class NeoAdvertisingContract : SmartContract
  {
    private static readonly byte[] GAS = { 231, 45, 40, 105, 121, 238, 108, 177, 183, 230, 93, 253, 223, 178, 227, 132, 16, 11, 141, 20, 142, 119, 88, 222, 66, 228, 22, 139, 113, 121, 44, 96 };

    // params: 0710
    // return : 05
    public static object Main(string operation, params object[] args)
    {
      //First argument is always the calling user
      byte[] user = (byte[])args[0];

      //Check if this is really the user to invoking this contract
      if (!Runtime.CheckWitness(user))
        return false;

      //Second argument is always the advertising ID
      string advertisingId = null;
      if (args.Length > 1)
        advertisingId = (string)args[1];

      switch (operation)
      {
        case "create":
          return CreateAdSpace(user, advertisingId, (uint)args[2]);
        case "minoffer":
          return GetMinOffer(advertisingId, (string)args[1]);
        case "buy":
          return BuyAdSpace(user, advertisingId, (uint)args[2], (string)args[3], (string)args[4], (string)args[5]);
        case "deposit":
          return Deposit(user);
        case "withdraw":
          return Withdraw(user);
        case "Profit":
          return GetProfit(user, advertisingId, (string)args[2]);
        default:
          return "Unknown Operation: " + operation;
      }

    }

    /// <summary>
    /// Creates a new ad space
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="name"></param>
    /// <param name="url"></param>
    /// <param name="minOffer"></param>
    /// <returns></returns>
    private static bool CreateAdSpace(byte[] owner, string advertisingId, uint minOffer)
    {
      //Check if Ad space already exist
      byte[] value = Storage.Get(Storage.CurrentContext, advertisingId);
      if (value != null)
        return false;

      //Create adspace in storage
      SaveAdSpace(advertisingId, minOffer);

      return true;
    }

    /// <summary>
    /// Store adspace in storage with minimum amount for advertisers
    /// </summary>
    /// <param name="id"></param>
    /// <param name="min"></param>
    private static void SaveAdSpace(string id, uint min)
    {
      Storage.Put(Storage.CurrentContext, id, min);
    }

    /// <summary>
    /// Get adspace minimum offer
    /// </summary>
    /// <param name="advertisingId"></param>
    /// <returns></returns>
    private static BigInteger GetAdSpaceMinAmount(string advertisingId)
    {
      byte[] value = Storage.Get(Storage.CurrentContext, advertisingId);
      if (value != null)
        return 0;
      else
      {
        return value.AsBigInteger();
      }
    }


    /// <summary>
    /// Get the minimum offer for this ad on the given date
    /// </summary>
    /// <param name="advertisingId"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    private static BigInteger GetMinOffer(string advertisingId, string date)
    {
      //Get the minimum offer for this ad on the given date
      var minOffer = GetAdSpaceMinAmount(advertisingId);

      //Check if there are offers on this date
      var currentOffer = GetOfferForDate(advertisingId, date);

      //Check the min amount for the advertisingId
      if (currentOffer > minOffer)
        return currentOffer;
      else
        return minOffer;
    }

    /// <summary>
    /// Get current offer amount on specified date
    /// </summary>
    /// <param name="advertisingId"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    private static BigInteger GetOfferForDate(string advertisingId, string date)
    {
      string index = GetOfferIndexName(advertisingId, date);
      byte[] value = Storage.Get(Storage.CurrentContext, index);
      if (value != null)
        return 0;
      else
      {
        return value.AsBigInteger();
      }
    }

    /// <summary>
    /// Store the offer for this advertigingId in storage
    /// </summary>
    /// <param name="advertiser"></param>
    /// <param name="advertisingId"></param>
    /// <param name="date"></param>
    /// <param name="amount"></param>
    private static void SetOfferForDate(byte[] advertiser, string advertisingId, string date, BigInteger amount)
    {
      string index = GetOfferIndexName(advertisingId, date);
      Storage.Put(Storage.CurrentContext, index, amount);

      string lockedIndex = GetLockedIndexName(advertisingId, date);
      Storage.Put(Storage.CurrentContext, lockedIndex, advertiser);

    }

    /// <summary>
    /// Try to buy ad space
    /// </summary>
    /// <param name="advertiser"></param>
    /// <param name="advertisingId"></param>
    /// <param name="amount"></param>
    /// <param name="date">Format: yyyymmdd</param>
    /// <param name="text"></param>
    /// <param name="url"></param>
    /// <returns></returns>
    private static bool BuyAdSpace(byte[] advertiser, string advertisingId, uint amount, string date, string text, string url)
    {
      //Text, url and date are mandatory
      if (advertisingId == null || text == null || url == null || date == null)
        return false;

      //Should always have 8 chars, yyyymmdd
      if (date.Length != 8)
        return false;

      //Check minimum offer
      var minOffer = GetMinOffer(advertisingId, date);

      //Offer should be at least the min offer
      if (amount < minOffer)
        return false;

      //Check if there are enough funds for this user
      var balance = GetBalance(advertiser);
      if (balance < amount)
        return false;

      //Lock funds
      //Unlock funds of any other person that had a lower amount on this dat

      //Register buy
      SetOfferForDate(advertiser, advertisingId, date, amount);

      return true;
    }

    /// <summary>
    /// Allow advertiser to withdraw GAS profits for selected date
    /// </summary>
    /// <param name="user"></param>
    /// <param name="advertisingId"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    private static object GetProfit(byte[] user, string advertisingId, string date)
    {
      //Should always have 8 chars, yyyymmdd
      if (date.Length != 8)
        return false;

      //Check how much we can withdraw
      BigInteger offer = GetOfferForDate(advertisingId, date);
      if (offer <= 0)
        return false;

      return true;
    }

    /// <summary>
    /// Withdraw unused funds
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    private static object Withdraw(byte[] user)
    {
      //TODO: Only allowed to withdraw unlocked funds

      //TODO: Check locked funds

      return false;
    }

    /// <summary>
    /// Deposit funds
    /// Inspired by: https://github.com/birmas/neotrade/blob/master/neotrade.cs
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    private static bool Deposit(byte[] owner)
    {
      Transaction tx = (Transaction)ExecutionEngine.ScriptContainer;
      TransactionOutput reference = tx.GetReferences()[0];

      if (reference.AssetId != GAS)
      {
        //Only GAS is allowed for payment
        return false;
      }

      TransactionOutput[] outputs = tx.GetOutputs();
      byte[] sender = reference.ScriptHash;
      byte[] receiver = ExecutionEngine.ExecutingScriptHash;
      ulong receivedGAS = 0;

      // Get all GAS for this transaction
      foreach (TransactionOutput output in outputs)
      {
        //Only gas for us
        if (output.ScriptHash == receiver)
        {
          ulong addedGas = (ulong)output.Value;

          //Only add GAS
          if (reference.AssetId == GAS)
            receivedGAS += addedGas;
        }
      }

      if (receivedGAS > 0)
      {
        DepositGas(sender, receivedGAS);
      }

      return true;
    }

    /// <summary>
    /// Store the address and amount of GAS deposited
    /// </summary>
    /// <param name="address"></param>
    /// <param name="newFunds"></param>
    public static void DepositGas(byte[] address, BigInteger newFunds)
    {
      byte[] gasBalanceIndex = GetBalanceIndexName(address);
      BigInteger current = GetBalance(address);

      BigInteger newBalance = current + newFunds;

      Storage.Put(Storage.CurrentContext, gasBalanceIndex, newBalance);

    }

    /// <summary>
    /// Get the balance for address
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static BigInteger GetBalance(byte[] address)
    {
      byte[] gasIndex = GetBalanceIndexName(address);

      BigInteger balance = Storage.Get(Storage.CurrentContext, gasIndex).AsBigInteger();

      return balance;
    }

    /// <summary>
    /// Get Balance index name
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static byte[] GetBalanceIndexName(byte[] address)
    {
      return address.Concat(GAS);
    }

    /// <summary>
    /// Get offer index name
    /// </summary>
    /// <param name="advertisingId"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    private static string GetOfferIndexName(string advertisingId, string date)
    {
      return advertisingId + "-" + date;
    }

    /// <summary>
    /// Get locked index name
    /// </summary>
    /// <param name="advertisingId"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    private static string GetLockedIndexName(string advertisingId, string date)
    {
      return advertisingId + "-" + date + "-locked";
    }
  }
}
