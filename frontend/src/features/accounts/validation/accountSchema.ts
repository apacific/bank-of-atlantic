import { z } from "zod";
import { isCreditAccountType, isDepositAccountType } from "../accountTypes";

export const accountCreateSchema = z
  .object({
    accountType: z.string().min(1, "Account type is required."),
    initialBalance: z.coerce.number().min(0, "Initial balance must be >= 0.").optional(),
    creditLimit: z.coerce.number().min(0, "Credit limit must be >= 0.").optional(),
  })
  .superRefine((v, ctx) => {
    if (isDepositAccountType(v.accountType)) {
      if (v.creditLimit !== undefined && v.creditLimit !== 0) {
        ctx.addIssue({ code: "custom", path: ["creditLimit"], message: "Credit limit is only for credit accounts." });
      }
    }

    if (isCreditAccountType(v.accountType)) {
      if (v.creditLimit === undefined || v.creditLimit <= 0) {
        ctx.addIssue({ code: "custom", path: ["creditLimit"], message: "Credit limit is required and must be > 0." });
      }
      if (v.initialBalance !== undefined && v.initialBalance !== 0) {
        ctx.addIssue({ code: "custom", path: ["initialBalance"], message: "Initial balance is only for deposit accounts." });
      }
    }
  });

export const accountUpdateSchema = z.object({
  accountType: z.string().min(1, "Account type is required."),
  availableBalance: z.coerce.number().min(0, "Value must be >= 0."),
});
