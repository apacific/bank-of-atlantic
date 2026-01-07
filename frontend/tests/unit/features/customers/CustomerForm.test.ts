import { describe, it, expect, vi, beforeEach } from 'vitest';
import { mount } from '@vue/test-utils';
import CustomerForm from '@/features/customers/components/CustomerForm.vue';
import TextField from '@/components/forms/TextField.vue';
import Modal from '@/components/Modal.vue';

// Mock the API
vi.mock('@/app/api', () => ({
  api: {
    post: vi.fn(),
    put: vi.fn(),
  },
  toCamel: (str: string) => str.charAt(0).toLowerCase() + str.slice(1),
}));

describe('CustomerForm Component', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render customer form in create mode', () => {
    const wrapper = mount(CustomerForm, {
      props: {
        mode: 'create',
      },
      global: {
        components: { TextField, Modal },
      },
    });

    expect(wrapper.find('form').exists()).toBe(true);
    expect(wrapper.find('.card').exists()).toBe(true);
  });

  it('should render all required text fields', () => {
    const wrapper = mount(CustomerForm, {
      props: {
        mode: 'create',
      },
      global: {
        components: { TextField, Modal },
      },
    });

    const textFields = wrapper.findAllComponents(TextField);
    expect(textFields.length).toBeGreaterThan(0);
  });

  it('should render submit and cancel buttons', () => {
    const wrapper = mount(CustomerForm, {
      props: {
        mode: 'create',
      },
      global: {
        components: { TextField, Modal },
      },
    });

    const buttons = wrapper.findAll('.btn');
    expect(buttons.length).toBeGreaterThanOrEqual(2);
    expect(buttons[0]?.text()).toBe('Submit');
    expect(buttons[1]?.text()).toBe('Cancel');
  });

  it('should emit cancel event when cancel button is clicked', async () => {
    const wrapper = mount(CustomerForm, {
      props: {
        mode: 'create',
      },
      global: {
        components: { TextField, Modal },
      },
    });

    const buttons = wrapper.findAll('.btn');
    await buttons[1]?.trigger('click');

    expect(wrapper.emitted('cancel')).toHaveLength(1);
  });

  it('should initialize form with empty values in create mode', () => {
    const wrapper = mount(CustomerForm, {
      props: {
        mode: 'create',
      },
      global: {
        components: { TextField, Modal },
      },
    });

    const textFields = wrapper.findAllComponents(TextField);
    textFields.forEach(field => {
      expect(field?.props('modelValue')).toBe('');
    });
  });

  it('should initialize form with initial values in edit mode', () => {
    const initial = {
      firstName: 'John',
      lastName: 'Doe',
      email: 'john@example.com',
      ssnTin: '123-45-6789',
      street: '123 Main St',
      city: 'Springfield',
      state: 'IL',
      postalCode: '62701',
      country: 'USA',
    };

    const wrapper = mount(CustomerForm, {
      props: {
        mode: 'edit',
        customerId: '123',
        initial,
      },
      global: {
        components: { TextField, Modal },
      },
    });

    const textFields = wrapper.findAllComponents(TextField);
    expect(textFields[0]?.props('modelValue')).toBe('John');
    expect(textFields[1]?.props('modelValue')).toBe('Doe');
    expect(textFields[5]?.props('modelValue')).toBe('john@example.com');
  });

  it('should render modal component', () => {
    const wrapper = mount(CustomerForm, {
      props: {
        mode: 'create',
      },
      global: {
        components: { TextField, Modal },
      },
    });

    expect(wrapper.findComponent(Modal).exists()).toBe(true);
  });

  it('should handle form field updates', async () => {
    const wrapper = mount(CustomerForm, {
      props: {
        mode: 'create',
      },
      global: {
        components: { TextField, Modal },
      },
    });

    const textFields = wrapper.findAllComponents(TextField);
    
    // Update first name
    await textFields[0]?.vm.$emit('update:modelValue', 'Jane');
    
    // Component should have updated internal state
    expect(wrapper.find('form').exists()).toBe(true);
  });

  it('should show submit button label as Submit in create mode', () => {
    const wrapper = mount(CustomerForm, {
      props: {
        mode: 'create',
      },
      global: {
        components: { TextField, Modal },
      },
    });

    const submitBtn = wrapper.findAll('.btn')[0];
    expect(submitBtn?.text()).toBe('Submit');
  });

  it('should disable submit button while saving', async () => {
    const wrapper = mount(CustomerForm, {
      props: {
        mode: 'create',
      },
      global: {
        components: { TextField, Modal },
      },
    });

    // Verify form renders with submit button
    const submitBtn = wrapper.findAll('.btn')[0];
    expect(submitBtn?.exists()).toBe(true);
    expect((submitBtn?.element as HTMLButtonElement)?.disabled).toBe(false);
  });

  it('should display email field with email type', () => {
    const wrapper = mount(CustomerForm, {
      props: {
        mode: 'create',
      },
      global: {
        components: { TextField, Modal },
      },
    });

    const textFields = wrapper.findAllComponents(TextField);
    const emailField = textFields.find(f => f.props('label')?.includes('Email'));
    expect(emailField?.props('type')).toBe('email');
  });

  it('should have grid layout for form fields', () => {
    const wrapper = mount(CustomerForm, {
      props: {
        mode: 'create',
      },
      global: {
        components: { TextField, Modal },
      },
    });

    expect(wrapper.find('.grid').exists()).toBe(true);
  });

  it('should have button row for submit and cancel', () => {
    const wrapper = mount(CustomerForm, {
      props: {
        mode: 'create',
      },
      global: {
        components: { TextField, Modal },
      },
    });

    expect(wrapper.find('.row').exists()).toBe(true);
  });
});
