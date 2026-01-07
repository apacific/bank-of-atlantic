export const DEPOSIT_ACCOUNT_CODES = ["Checking", "Savings", "MoneyMarket", "CD"] as const;
export const CREDIT_ACCOUNT_CODES  = ["CreditCard", "HELOC", "PLOC"] as const;

export type DepositAccountCode = typeof DEPOSIT_ACCOUNT_CODES[number];
export type CreditAccountCode  = typeof CREDIT_ACCOUNT_CODES[number];
export type AccountTypeCode    = DepositAccountCode | CreditAccountCode;

export const accountTypeOptions: { label: string; value: AccountTypeCode }[] = [
  { label: "Checking",      value: "Checking" },
  { label: "Savings",       value: "Savings" },
  { label: "Money Market",  value: "MoneyMarket" },
  { label: "CD",            value: "CD" },
  { label: "Credit Card",   value: "CreditCard" },
  { label: "HELOC",         value: "HELOC" },
  { label: "PLOC",          value: "PLOC" },
];

export function isCreditAccountType(code: string): code is CreditAccountCode {
  return (CREDIT_ACCOUNT_CODES as readonly string[]).includes(code);
}

export function isDepositAccountType(code: string): code is DepositAccountCode {
  return (DEPOSIT_ACCOUNT_CODES as readonly string[]).includes(code);
}

export function formatAccountTypeLabel(code: string): string {
  const match = accountTypeOptions.find(o => o.value === code);
  return match?.label ?? code;
}
