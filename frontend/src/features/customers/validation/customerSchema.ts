import { z } from "zod";

export const customerSchema = z.object({
  firstName: z.string().trim().min(1, "First name is required."),
  lastName: z.string().trim().min(1, "Last name is required."),
  suffix: z.string().trim().max(20, "Suffix is too long.").default(""),
  title: z.string().trim().max(30, "Title is too long.").default(""),

  ssnTin: z.string().trim().min(1, "SSN/TIN is required."),
  email: z.string().trim().min(1, "Email is required.").email("Email must be valid."),

  street: z.string().trim().min(1, "Street is required."),
  city: z.string().trim().min(1, "City is required."),
  state: z.string().trim().min(1, "State is required."),
  postalCode: z.string().trim().min(1, "ZIP / Postal code is required."),
  country: z.string().trim().min(1, "Country is required."),
});

export type CustomerFormModel = z.output<typeof customerSchema>;
