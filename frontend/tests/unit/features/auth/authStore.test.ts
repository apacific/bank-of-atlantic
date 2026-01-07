import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import { useAuth } from '@/features/auth/authStore'

/**
 * Unit tests for the auth store (useAuth composable).
 * Tests the authentication state management: session setting/clearing, computed properties.
 */

describe('authStore - useAuth', () => {
  beforeEach(() => {
    // Clear sessionStorage before each test
    sessionStorage.clear()
    vi.clearAllMocks()
    // Reset module cache to ensure fresh state for each test
    vi.resetModules()
  })

  afterEach(() => {
    sessionStorage.clear()
  })

  describe('setSession', () => {
    it('should set token and role in both ref and sessionStorage', () => {
      // Arrange
      const { setSession, token, role } = useAuth()
      const testToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test.signature'
      const testRole = 'Employee'

      // Act
      setSession(testToken, testRole)

      // Assert
      expect(token.value).toBe(testToken)
      expect(role.value).toBe(testRole)
      expect(sessionStorage.getItem('boa_token')).toBe(testToken)
      expect(sessionStorage.getItem('boa_role')).toBe(testRole)
    })

    it('should handle Manager role correctly', () => {
      // Arrange
      const { setSession, role } = useAuth()
      const managerRole = 'Manager'

      // Act
      setSession('test-token', managerRole)

      // Assert
      expect(role.value).toBe(managerRole)
      expect(sessionStorage.getItem('boa_role')).toBe(managerRole)
    })

    it('should overwrite previous session when called again', () => {
      // Arrange
      const { setSession, token, role } = useAuth()
      const oldToken = 'old-token'
      const newToken = 'new-token'

      // Act
      setSession(oldToken, 'Employee')
      setSession(newToken, 'Manager')

      // Assert
      expect(token.value).toBe(newToken)
      expect(role.value).toBe('Manager')
      expect(sessionStorage.getItem('boa_token')).toBe(newToken)
    })
  })

  describe('clearSession', () => {
    it('should clear token and role from both ref and sessionStorage', () => {
      // Arrange
      const { setSession, clearSession, token, role } = useAuth()
      setSession('test-token', 'Employee')

      // Act
      clearSession()

      // Assert
      expect(token.value).toBeNull()
      expect(role.value).toBeNull()
      expect(sessionStorage.getItem('boa_token')).toBeNull()
      expect(sessionStorage.getItem('boa_role')).toBeNull()
    })

    it('should not raise error when called without prior session', () => {
      // Arrange
      const { clearSession } = useAuth()

      // Act & Assert
      expect(() => clearSession()).not.toThrow()
      expect(sessionStorage.getItem('boa_token')).toBeNull()
    })
  })

  describe('isAuthenticated computed', () => {
    it('should return true when token is set', () => {
      // Arrange
      const { setSession, isAuthenticated } = useAuth()

      // Act
      setSession('test-token', 'Employee')

      // Assert
      expect(isAuthenticated.value).toBe(true)
    })

    it('should return false when token is not set', async () => {
      // Arrange: Reset modules to get fresh state
      vi.resetModules()
      sessionStorage.clear()

      // Import fresh instance
      const { useAuth: useAuthFresh } = await import('@/features/auth/authStore')
      const { isAuthenticated } = useAuthFresh()

      // Assert (token should be null/falsy initially)
      expect(isAuthenticated.value).toBe(false)
    })

    it('should return false after clearSession', () => {
      // Arrange
      const { setSession, clearSession, isAuthenticated } = useAuth()
      setSession('test-token', 'Employee')

      // Act
      clearSession()

      // Assert
      expect(isAuthenticated.value).toBe(false)
    })

    it('should reflect token changes reactively', () => {
      // Arrange
      const { token, isAuthenticated } = useAuth()

      // Assert
      expect(isAuthenticated.value).toBe(false)

      // Act
      token.value = 'test-token'

      // Assert
      expect(isAuthenticated.value).toBe(true)
    })
  })

  describe('persistent storage recovery', () => {
    it('should recover session from sessionStorage on composable creation', async () => {
      // Arrange: Set session in storage
      sessionStorage.setItem('boa_token', 'persisted-token')
      sessionStorage.setItem('boa_role', 'Manager')

      // Clear module cache to ensure fresh import
      vi.resetModules()
      
      // Import fresh instance
      const { useAuth: useAuthFresh } = await import('@/features/auth/authStore')
      const { token, role, isAuthenticated } = useAuthFresh()

      // Assert
      expect(token.value).toBe('persisted-token')
      expect(role.value).toBe('Manager')
      expect(isAuthenticated.value).toBe(true)
    })

    it('should handle missing token in storage gracefully', async () => {
      // Arrange: Set only role in storage
      sessionStorage.setItem('boa_role', 'Manager')
      // boa_token not set

      // Clear module cache to ensure fresh import
      vi.resetModules()
      
      // Import fresh instance
      const { useAuth: useAuthFresh } = await import('@/features/auth/authStore')
      const { token, role, isAuthenticated } = useAuthFresh()

      // Assert
      expect(token.value).toBeNull()
      expect(role.value).toBe('Manager')
      expect(isAuthenticated.value).toBe(false)
    })
  })
})
