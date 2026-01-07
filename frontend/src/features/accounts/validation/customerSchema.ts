import { z } from "zod";

const notBlank = (m: string) => z.string().trim().min(1, m);

export const customerSchema = z.object({
  firstName: notBlank("First name is required."),
  lastName: notBlank("Last name is required."),
  suffix: z.string().trim().optional(),
  title: z.string().trim().optional(),
  ssnTin: notBlank("SSN/TIN is required."),
  email: notBlank("Email is required.").email("Email format is invalid."),
  street: notBlank("Street is required."),
  city: notBlank("City is required."),
  state: notBlank("State is required."),
  postalCode: notBlank("ZIP/Postal code is required."),
  country: notBlank("Country is required."),
});
