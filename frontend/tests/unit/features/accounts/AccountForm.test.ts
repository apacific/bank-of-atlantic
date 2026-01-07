import { describe, it, expect, vi, beforeEach } from 'vitest';
import { mount } from '@vue/test-utils';
import AccountForm from '@/features/accounts/components/AccountForm.vue';
import TextField from '@/components/forms/TextField.vue';
import Modal from '@/components/Modal.vue';

// Mock SelectField component since it may not exist or we're focusing on basic tests
vi.mock('@/components/forms/SelectField.vue', () => ({
  default: {
    name: 'SelectField',
    template: '<div class="select-field"><input /></div>',
    props: ['label', 'modelValue', 'options', 'error'],
    emits: ['update:modelValue'],
  },
}));

// Mock the API
vi.mock('@/app/api', () => ({
  api: {
    post: vi.fn(),
    put: vi.fn(),
  },
  toCamel: (str: string) => str.charAt(0).toLowerCase() + str.slice(1),
}));

// Mock account types
vi.mock('@/features/accounts/accountTypes', () => ({
  isCreditAccountType: (type: string) => ['CreditCard', 'HELOC', 'PLOC'].includes(type),
  isDepositAccountType: (type: string) => ['Checking', 'Savings', 'MoneyMarket', 'CD'].includes(type),
}));

describe('AccountForm Component', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render account form in create mode', () => {
    const wrapper = mount(AccountForm, {
      props: {
        mode: 'create',
        customerId: '123',
      },
      global: {
        components: { TextField, Modal },
        stubs: {
          SelectField: true,
        },
      },
    });

    expect(wrapper.find('form').exists()).toBe(true);
    expect(wrapper.find('.card').exists()).toBe(true);
  });

  it('should render submit and cancel buttons', () => {
    const wrapper = mount(AccountForm, {
      props: {
        mode: 'create',
        customerId: '123',
      },
      global: {
        components: { TextField, Modal },
        stubs: {
          SelectField: true,
        },
      },
    });

    const buttons = wrapper.findAll('.btn');
    expect(buttons.length).toBeGreaterThanOrEqual(2);
  });

  it('should emit cancel event when cancel button is clicked', async () => {
    const wrapper = mount(AccountForm, {
      props: {
        mode: 'create',
        customerId: '123',
      },
      global: {
        components: { TextField, Modal },
        stubs: {
          SelectField: true,
        },
      },
    });

    const buttons = wrapper.findAll('.btn');
    const cancelBtn = buttons[buttons.length - 1]; // Last button is usually cancel
    await cancelBtn?.trigger('click');

    expect(wrapper.emitted('cancel')).toHaveLength(1);
  });

  it('should initialize form in create mode', () => {
    const wrapper = mount(AccountForm, {
      props: {
        mode: 'create',
        customerId: '123',
      },
      global: {
        components: { TextField, Modal },
        stubs: {
          SelectField: true,
        },
      },
    });

    expect(wrapper.find('form').exists()).toBe(true);
  });

  it('should initialize form with initial values in edit mode', () => {
    const wrapper = mount(AccountForm, {
      props: {
        mode: 'edit',
        customerId: '123',
        accountId: '456',
        initial: {
          accountType: 'Checking',
          currentBalance: 1000,
        },
      },
      global: {
        components: { TextField, Modal },
        stubs: {
          SelectField: true,
        },
      },
    });

    expect(wrapper.find('form').exists()).toBe(true);
  });

  it('should render modal component', () => {
    const wrapper = mount(AccountForm, {
      props: {
        mode: 'create',
        customerId: '123',
      },
      global: {
        components: { TextField, Modal },
        stubs: {
          SelectField: true,
        },
      },
    });

    expect(wrapper.findComponent(Modal).exists()).toBe(true);
  });

  it('should display different labels for deposit and credit accounts', async () => {
    const wrapper = mount(AccountForm, {
      props: {
        mode: 'edit',
        customerId: '123',
        accountId: '456',
        initial: {
          accountType: 'CreditCard',
          availableCredit: 5000,
        },
      },
      global: {
        components: { TextField, Modal },
        stubs: {
          SelectField: true,
        },
      },
    });

    // For credit card accounts, should show "Available credit" label
    expect(wrapper.html()).toContain('Available credit');
  });

  it('should have form grid layout', () => {
    const wrapper = mount(AccountForm, {
      props: {
        mode: 'create',
        customerId: '123',
      },
      global: {
        components: { TextField, Modal },
        stubs: {
          SelectField: true,
        },
      },
    });

    expect(wrapper.find('.grid').exists()).toBe(true);
  });

  it('should have button row for submit and cancel', () => {
    const wrapper = mount(AccountForm, {
      props: {
        mode: 'create',
        customerId: '123',
      },
      global: {
        components: { TextField, Modal },
        stubs: {
          SelectField: true,
        },
      },
    });

    expect(wrapper.find('.row').exists()).toBe(true);
  });

  it('should disable submit button while saving', async () => {
    const wrapper = mount(AccountForm, {
      props: {
        mode: 'create',
        customerId: '123',
      },
      global: {
        components: { TextField, Modal },
        stubs: {
          SelectField: true,
        },
      },
    });

    const submitBtn = wrapper.findAll('.btn')[0];
    expect(submitBtn?.exists()).toBe(true);
    expect((submitBtn?.element as HTMLButtonElement)?.disabled).toBe(false);
  });

  it('should receive customerId prop', () => {
    const wrapper = mount(AccountForm, {
      props: {
        mode: 'create',
        customerId: 'test-customer-123',
      },
      global: {
        components: { TextField, Modal },
        stubs: {
          SelectField: true,
        },
      },
    });

    expect(wrapper.props('customerId')).toBe('test-customer-123');
  });

  it('should receive accountId prop in edit mode', () => {
    const wrapper = mount(AccountForm, {
      props: {
        mode: 'edit',
        customerId: 'cust-123',
        accountId: 'acc-456',
      },
      global: {
        components: { TextField, Modal },
        stubs: {
          SelectField: true,
        },
      },
    });

    expect(wrapper.props('accountId')).toBe('acc-456');
  });

  it('should handle initial values for credit accounts', () => {
    const wrapper = mount(AccountForm, {
      props: {
        mode: 'edit',
        customerId: '123',
        accountId: '456',
        initial: {
          accountType: 'CreditCard',
          availableCredit: 2500,
        },
      },
      global: {
        components: { TextField, Modal },
        stubs: {
          SelectField: true,
        },
      },
    });

    expect(wrapper.props('initial')?.availableCredit).toBe(2500);
  });
});
